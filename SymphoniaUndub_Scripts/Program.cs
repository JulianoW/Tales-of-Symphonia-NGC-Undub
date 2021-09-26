using System;
using System.IO;

namespace SymphoniaUndub_Scripts
{
    class Program
    {
        // Gamecube is big endian... grr!
        public static UInt32 BigEndianReadUInt32(BinaryReader br)
        {
            var data = br.ReadBytes(4);
            Array.Reverse(data);
            return BitConverter.ToUInt32(data, 0);
        }

        static void Main(string[] args)
        {
            // replace this with directory you're using to store files
            // as you can see, my old Symph files were in an old backup of an old backup XD
            string pwd = @"D:\BackUp\backupC\btlenemy";

            // You will need the Japanese and English btlenemy and btlusual.
            // The code expects the following files in this directory:
            //      NABTLenemy.dat
            //      NABTLusual.dat
            //      JPBTLenemy.dat
            //      JPBTLusual.dat

            // Need to get pointers:
            // NA Ptrs btlusual 0xE3B40  - 0x3F0 -- 252 times/FC
            // JP Ptrs btlusual 0xE2D20  - 0x3F0 -- 252 times/FC

            FileStream nafs = new FileStream($@"{pwd}\NABTLusual.dat", FileMode.Open);
            BinaryReader nabr = new BinaryReader(nafs);
            nabr.BaseStream.Seek(0xE3B40, SeekOrigin.Begin);

            FileStream jpfs = new FileStream($@"{pwd}\JPBTLusual.dat", FileMode.Open);
            BinaryReader jpbr = new BinaryReader(jpfs);
            jpbr.BaseStream.Seek(0xE2D20, SeekOrigin.Begin);

            UInt32[] NA_Pointers = new UInt32[252];
            UInt32[] JP_Pointers = new UInt32[252];

            // read pointers (big endian)
            for (int i = 0; i < 252; i++)
            {
                NA_Pointers[i] = BigEndianReadUInt32(nabr);
                JP_Pointers[i] = BigEndianReadUInt32(jpbr);
            }


            // open btlenemy's to start parsing data
            nafs = new FileStream($@"{pwd}\NABTLenemy.dat", FileMode.Open);
            nabr = new BinaryReader(nafs);
            jpfs = new FileStream($@"{pwd}\JPBTLenemy.dat", FileMode.Open);
            jpbr = new BinaryReader(jpfs);


            // loop through btlenemy NA --> write enc to file --> decrypt --> write decrypt to file
            // loop through btlenemy JP --> write enc to file --> decrypt --> write decrypt to file
            for (int i = 0; i < 251; i++)
            {
                // get pointers
                UInt32 base_na_ptr = NA_Pointers[i];
                UInt32 base_jp_ptr = JP_Pointers[i];

                // get size (end pointer - current pointer) --- last pointer points to EOF so this is OK
                int na_size = (int)(NA_Pointers[i + 1] - NA_Pointers[i]);
                int jp_size = (int)(JP_Pointers[i + 1] - JP_Pointers[i]);

                // seek to pointer
                nabr.BaseStream.Seek(base_na_ptr, SeekOrigin.Begin);
                jpbr.BaseStream.Seek(base_jp_ptr, SeekOrigin.Begin);

                // write files into individual files
                BinaryWriter nabw = new BinaryWriter(File.Open($@"{pwd}\{i}_NA.bin", System.IO.FileMode.Create));
                BinaryWriter jpbw = new BinaryWriter(File.Open($@"{pwd}\{i}_JP.bin", System.IO.FileMode.Create));

                nabw.Write(nabr.ReadBytes((int)na_size));
                jpbw.Write(jpbr.ReadBytes((int)jp_size));

                nabw.Flush();
                jpbw.Flush();
                nabw.Close();
                jpbw.Close();

                // decompress the output file
                complib.DecodeFile(@$"{pwd}\{i}_NA.bin", @$"{pwd}\{i}_NA_dec.bin", 0, 3, true);
                complib.DecodeFile(@$"{pwd}\{i}_JP.bin", @$"{pwd}\{i}_JP_dec.bin", 0, 3, true);
            }

            int found_count = 0;
            // loop through an index
            for (int i = 0; i < 251; i++)
            {
                nafs = new FileStream($@"{pwd}\{i}_NA_dec.bin", FileMode.Open);
                nabr = new BinaryReader(nafs);
                jpfs = new FileStream($@"{pwd}\{i}_JP_dec.bin", FileMode.Open);
                jpbr = new BinaryReader(jpfs);

                // load NA, load JP
                // read pointer to voice data at 0x1E4 (big endian)
                nabr.BaseStream.Seek(0x1E4, SeekOrigin.Begin);
                jpbr.BaseStream.Seek(0x1E4, SeekOrigin.Begin);

                UInt32 na_ptr = BigEndianReadUInt32(nabr);
                UInt32 jp_ptr = BigEndianReadUInt32(jpbr);
                
                if (na_ptr == 0 && jp_ptr == 0)
                {
                    // if pointer is 0, no voice data - continue to next loop iteration
                    Console.WriteLine($"Enemy id {i} - No Voice Data");
                    continue;
                }
                found_count++;
                Console.WriteLine($"Enemy id {i} - Voice Data Found -- #{found_count}");
                
                // seek to voice data
                //nabr.BaseStream.Seek(na_ptr, SeekOrigin.Begin);
                jpbr.BaseStream.Seek(jp_ptr, SeekOrigin.Begin);

                // write out a modified NA decrypted file
                BinaryWriter nabw = new BinaryWriter(File.Open($@"{pwd}\{i}_NA_dec_modified.bin", System.IO.FileMode.Create));
                nabr.BaseStream.Seek(0, SeekOrigin.Begin);
                // Write all data up to voice data portion
                nabw.Write(nabr.ReadBytes((int)(na_ptr)));
                // Voice data is last piece of the file. (I didn't look everywhere so it could be there may be some exceptions..???)
                // Write the voice section to EOF of JP file into the modified NA file
                nabw.Write(jpbr.ReadBytes((int)(jpbr.BaseStream.Length - jpbr.BaseStream.Position)));
                
                // this is old code from when I was trying to do a more selective replacement instead of just taking the entire JP section
                
                // at 0xB and 0xC there are two bytes that make up a pointer to a table with voice details (and pointers to vbank)
                /*// read A bytes
                nabr.ReadBytes(0xA);
                jpbr.ReadBytes(0xA);

                // get 1st byte of pointer
                byte na_1 = nabr.ReadByte();
                byte jp_1 = jpbr.ReadByte();
                // read A bytes
                //nabr.ReadBytes(0xA);
                //jpbr.ReadBytes(0xA);

                // get 2nd byte of pointer
                byte na_2 = nabr.ReadByte();
                byte jp_2 = jpbr.ReadByte();

                // shift first pointer left and add 2nd. I guess I could have done a readuin16, but this is big endian again
                // and I originally have the wrong second byte and it wasn't consecutive.
                int na_table_ptr = (na_1 << 8) + na_2;
                int jp_table_ptr = (jp_1 << 8) + jp_2;


                BinaryWriter nabw = new BinaryWriter(File.Open($@"{pwd}\{i}_NA_dec_modified.bin", System.IO.FileMode.Create));
                jpbr.BaseStream.Seek(0, SeekOrigin.Begin);
                nabw.Write(jpbr.ReadBytes((int)(na_ptr + na_table_ptr)));

                nabr.BaseStream.Seek(na_ptr + na_table_ptr, SeekOrigin.Begin);
                jpbr.BaseStream.Seek(jp_ptr + jp_table_ptr, SeekOrigin.Begin);

                // read Uint32 -- if FFFF --> break
                while (true)
                {
                    UInt32 na_voice_id = nabr.ReadUInt32();
                    UInt32 jp_voice_id = jpbr.ReadUInt32();
                    //nabw.Write(na_voice_id);
                    nabw.Write(na_voice_id);
                    if (na_voice_id == 0xFFFFFFFF)
                    {
                        
                        // check if na and jp same - if not, error
                        if (na_voice_id != jp_voice_id)
                        {
                            Console.WriteLine("ERROR - MISMATCH ON NUMBER OF VOICES // Enemy id {i}");
                        }
                        // end point can break anyway
                        break;
                    }
                    else
                    {
                        // not FFFF, read 0x1C bytes to copy
                        nabw.Write(jpbr.ReadBytes(0x1C));
                        nabr.ReadBytes(0x1C);
                    }
                }
                // after breaking out, we just need to write rest of the binary
                nabw.Write(nabr.ReadBytes((int)(nabr.BaseStream.Length - nabr.BaseStream.Position)));

                */
                nabw.Flush();
                nabw.Close();
            }

            // loop back thru, rebuild NA btlenemy and btlusual
            // note I already replaced the btlvbank pointer table in btlusual, before running this script
            BinaryWriter enemybw = new BinaryWriter(File.Open($@"{pwd}\BTLenemy.dat", System.IO.FileMode.Create));
            BinaryWriter usualbw = new BinaryWriter(File.Open($@"{pwd}\BTLusual.dat", System.IO.FileMode.Open));
            // pointer to btlenemy pointer table in NA
            usualbw.BaseStream.Seek(0xE3B40, SeekOrigin.Begin);

            for (int i = 0; i < 251; i++)
            {
                UInt32 pos = (UInt32)enemybw.BaseStream.Position;
                
                // little endian grrr fadjshiufasdfsa convert to big endian
                byte[] pos_bytes = BitConverter.GetBytes(pos);
                Array.Reverse(pos_bytes);
                usualbw.Write(pos_bytes);

                string file = $@"{pwd}\{i}_NA_dec.bin";
                if (File.Exists($@"{pwd}\{i}_NA_dec_modified.bin"))
                {
                    file = $@"{pwd}\{i}_NA_dec_modified.bin";
                }

                // encode file
                complib.EncodeFile(file, $@"{pwd}\{i}_NA_modified_enc.bin",0,3,true);

                nafs = new FileStream($@"{pwd}\{i}_NA_modified_enc.bin", FileMode.Open);
                nabr = new BinaryReader(nafs);
                enemybw.Write(nabr.ReadBytes((int)nabr.BaseStream.Length));

                // write out rest of the 0x10 line with 0's
                while (enemybw.BaseStream.Position % 0x10 != 0)
                {
                    enemybw.Write(new byte[] { 0x0 });
                }
                // also write another 0x10 line of 0's - not doing this caused some issues
                enemybw.Write(new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 });
                enemybw.Flush();
                usualbw.Flush();
            }

            // write out the EOF pointer
            UInt32 pos2 = (UInt32)enemybw.BaseStream.Position;

            // little endian grrr fadjshiufasdfsa convert to big endian
            byte[] pos_bytes2 = BitConverter.GetBytes(pos2);
            Array.Reverse(pos_bytes2);
            usualbw.Write(pos_bytes2);

            enemybw.Flush();
            enemybw.Close();
            usualbw.Flush();
            usualbw.Close();

            // old code from when I was using my own decrypt code
            // note this required adding the "unsafe" modifier in the method declaration

            /*string infile = $@"D:\BackUp\backupC\footsoldier_JP.bin";
            string outfile = $@"D:\BackUp\backupC\lzss_footsoldier_JP.bin";
            
            // need to adjust this based on size
            byte[] p1 = new byte[0x100000];
            byte[] p4 = new byte[0x10000];
            string filepath = $@"D:\BackUp\backupC\footsoldier_JP.bin";
            //FileStream fs = new FileStream(filepath, FileMode.Open);
           // BinaryWriter bw = new BinaryWriter(File.Open(@"D:\BackUp\backupC\footsoldier_JP_dec2.bin", System.IO.FileMode.Create));
            //BinaryWriter bw2 = new BinaryWriter(File.Open(@"D:\BackUp\backupC\footsoldier_JP_dec_buff.bin", System.IO.FileMode.Create));
            complib.DecodeFile(infile, outfile, 0, 0, true);

            //BinaryReader br = new BinaryReader(fs);
            //br.ReadBytes(9);
            /*int filesize = (int)br.BaseStream.Length - 9;
            byte[] file = br.ReadBytes(filesize);
            fixed (byte* param1 = p1, param4 = p4, param2 = file)
            {
                byte* param3 = param2 + filesize;
                Decrypt.DoDecrypt(param1, param2, param3, param4);
                bw.Write(p1);
                bw.Flush();
                bw2.Write(p4);
                bw2.Flush();
            }*/
        }
    }

}
