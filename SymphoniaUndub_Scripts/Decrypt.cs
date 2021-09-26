using System;
using System.Collections.Generic;
using System.Text;

namespace SymphoniaUndub_Scripts
{
    public static class Decrypt
    {
        // This code was originally taken from the Ghidra decompilation of the game's decompression routine.
        // I started commenting, renaming variables, and decoupling variables that were used for multiple things.
        // Part way through, I learned this was the compto compression, so I stopped work on this.

        // I think it's interesting so I left it here!


        public static unsafe void DoDecrypt(byte* output, byte* ptr_btlenemy, byte* ptr_end, byte* buffer)
        { 
            //p1 is actual destination - dont care about p4
            //p2 is source btlenemy (after first 9 bytes)
            //p3 is end of btlenemy only used as end condition
            //p4 is stack/buffer byte[]
        
            uint flag_bits;
            byte* new_ptr;
            uint bottom;
            uint top;
            int index;
            int loop_counter;
            uint encode_value;
            uint uVar12;
            int char_to_write;
            byte first_value;
            byte second_value;
            uint uVar16;
            int end_index;
            int offset;
            uint uloop_counter;

            index = 0;
            end_index = 0x1fd;
            do
            {
                index = index + 8;
                end_index = end_index + -1;
            } while (end_index != 0);
            end_index = 0xfef - index;
            if (index < 0xfef)
            {
                do
                {
                    end_index = end_index + -1;
                } while (end_index != 0);
            }
            char_to_write = '\0';
            offset = 0;
            loop_counter = 0x80;
            do
            {
                // writing initial "nonsense" into buffer
                *(int*)(buffer + offset) = char_to_write;
                *(int*)(buffer + offset + 1) = 0;
                *(int*)(buffer + offset + 2) = char_to_write;
                *(int*)(buffer + offset + 3) = 0;
                *(int*)(buffer + offset + 4) = char_to_write;
                *(int*)(buffer + offset + 5) = 0;
                *(int*)(buffer + offset + 6) = char_to_write;
                char_to_write = char_to_write + '\x01';
                *(int*)(buffer + offset + 7) = 0;
                *(int*)(buffer + offset + 8) = char_to_write;
                *(int*)(buffer + offset + 9) = 0;
                *(int*)(buffer + offset + 10) = char_to_write;
                *(int*)(buffer + offset + 0xb) = 0;
                *(int*)(buffer + offset + 0xc) = char_to_write;
                *(int*)(buffer + offset + 0xd) = 0;
                *(int*)(buffer + offset + 0xe) = char_to_write;
                *(int*)(buffer + offset + 0xf) = 0;
                char_to_write = char_to_write + '\x01';
                offset = offset + 0x10;
                loop_counter = loop_counter + -1;
            } while (loop_counter != 0);
            char_to_write = '\0';
            loop_counter = 0x20;
            do
            {
                *(int*)(buffer + offset) = char_to_write;
                
                *(int*)(buffer + offset + 1) = 0xff;
                *(int*)(buffer + offset + 2) = char_to_write;
                *(int*)(buffer + offset + 3) = 0xff;
                *(int*)(buffer + offset + 4) = char_to_write;
                *(int*)(buffer + offset + 5) = 0xff;
                *(int*)(buffer + offset + 6) = char_to_write;
                char_to_write = char_to_write + '\x01';
                *(int*)(buffer + offset + 7) = char_to_write;
                *(int*)(buffer + offset + 8) = 0xff;
                *(int*)(buffer + offset + 9) = char_to_write;
                *(int*)(buffer + offset + 10) = 0xff;
                *(int*)(buffer + offset + 0xb) = char_to_write;
                *(int*)(buffer + offset + 0xc) = 0xff;
                *(int*)(buffer + offset + 0xd) = char_to_write;
                char_to_write = char_to_write + '\x01';
                *(int*)(buffer + offset + 0xe) = char_to_write;
                *(int*)(buffer + offset + 0xf) = 0xff;
                *(int*)(buffer + offset + 0x10) = char_to_write;
                *(int*)(buffer + offset + 0x11) = 0xff;
                *(int*)(buffer + offset + 0x12) = char_to_write;
                *(int*)(buffer + offset + 0x13) = 0xff;
                *(int*)(buffer + offset + 0x14) = char_to_write;
                char_to_write = char_to_write + '\x01';
                *(int*)(buffer + offset + 0x15) = char_to_write;
                *(int*)(buffer + offset + 0x16) = 0xff;
                *(int*)(buffer + offset + 0x17) = char_to_write;
                *(int*)(buffer + offset + 0x18) = 0xff;
                *(int*)(buffer + offset + 0x19) = char_to_write;
                *(int*)(buffer + offset + 0x1a) = 0xff;
                *(int*)(buffer + offset + 0x1b) = char_to_write;
                char_to_write = char_to_write + '\x01';
                *(int*)(buffer + offset + 0x1c) = char_to_write;
                *(int*)(buffer + offset + 0x1d) = 0xff;
                *(int*)(buffer + offset + 0x1e) = char_to_write;
                *(int*)(buffer + offset + 0x1f) = 0xff;
                *(int*)(buffer + offset + 0x20) = char_to_write;
                *(int*)(buffer + offset + 0x21) = 0xff;
                *(int*)(buffer + offset + 0x22) = char_to_write;
                char_to_write = char_to_write + '\x01';
                *(int*)(buffer + offset + 0x23) = char_to_write;
                *(int*)(buffer + offset + 0x24) = 0xff;
                *(int*)(buffer + offset + 0x25) = char_to_write;
                *(int*)(buffer + offset + 0x26) = 0xff;
                *(int*)(buffer + offset + 0x27) = char_to_write;
                *(int*)(buffer + offset + 0x28) = 0xff;
                *(int*)(buffer + offset + 0x29) = char_to_write;
                char_to_write = char_to_write + '\x01';
                *(int*)(buffer + offset + 0x2a) = char_to_write;
                *(int*)(buffer + offset + 0x2b) = 0xff;
                *(int*)(buffer + offset + 0x2c) = char_to_write;
                *(int*)(buffer + offset + 0x2d) = 0xff;
                *(int*)(buffer + offset + 0x2e) = char_to_write;
                *(int*)(buffer + offset + 0x2f) = 0xff;
                *(int*)(buffer + offset + 0x30) = char_to_write;
                char_to_write = char_to_write + '\x01';
                *(int*)(buffer + offset + 0x31) = char_to_write;
                *(int*)(buffer + offset + 0x32) = 0xff;
                *(int*)(buffer + offset + 0x33) = char_to_write;
                *(int*)(buffer + offset + 0x34) = 0xff;
                *(int*)(buffer + offset + 0x35) = char_to_write;
                *(int*)(buffer + offset + 0x36) = 0xff;
                *(int*)(buffer + offset + 0x37) = char_to_write;
                char_to_write = char_to_write + '\x01';
                offset = offset + 0x38;
                loop_counter = loop_counter + -1;
            } while (loop_counter != 0);
            
            uVar12 = 0xfef;
            // flag bits tells which logic we do - init 0
            flag_bits = 0;
            // finished setup
            // now we decrypt
            while (ptr_btlenemy < ptr_end)
            {
                flag_bits = flag_bits >> 1;
                new_ptr = ptr_btlenemy;
                if ((flag_bits & 0x100) == 0)
                {
                    new_ptr = ptr_btlenemy + 1;
                    flag_bits = (uint)(*ptr_btlenemy | 0xff00); //assign new flag bits
                }
                if ((flag_bits & 1) == 0)
                {
                    first_value = *new_ptr;
                    second_value = new_ptr[1];
                    ptr_btlenemy = new_ptr + 2;
                    bottom = (uint)(second_value & 0xf);
                    top = (uint)((second_value & 0xf0) << 4);
                    encode_value = first_value | top;
                    if (bottom < 0xF)
                    {
                        top = 0;
                        if (bottom > 5)
                        {
                            // copy data out of buffer
                            uloop_counter = bottom + 2 >> 3; // this will be either 2 (E) or 1 (others)
                            do
                            {
                                byte output_value;
                                uVar16 = encode_value + top;
                                top = top + 8;
                                output_value = *(byte*)(buffer + (uVar16 & 0xfff));
                                *(byte*)(buffer + uVar12) = output_value;
                                *output = output_value;
                                output_value = *(byte*)(buffer + (uVar16 + 1 & 0xfff));
                                uVar12 = uVar12 + 1 & 0xfff;
                                *(byte*)(buffer + uVar12) = output_value;
                                uVar12 = uVar12 + 1 & 0xfff;
                                output[1] = output_value;
                                output_value = *(byte*)(buffer + (uVar16 + 2 & 0xfff));
                                *(byte*)(buffer + uVar12) = output_value;
                                uVar12 = uVar12 + 1 & 0xfff;
                                output[2] = output_value;
                                output_value = *(byte*)(buffer + (uVar16 + 3 & 0xfff));
                                *(byte*)(buffer + uVar12) = output_value;
                                uVar12 = uVar12 + 1 & 0xfff;
                                output[3] = output_value;
                                output_value = *(byte*)(buffer + (uVar16 + 4 & 0xfff));
                                *(byte*)(buffer + uVar12) = output_value;
                                uVar12 = uVar12 + 1 & 0xfff;
                                output[4] = output_value;
                                output_value = *(byte*)(buffer + (uVar16 + 5 & 0xfff));
                                *(byte*)(buffer + uVar12) = output_value;
                                uVar12 = uVar12 + 1 & 0xfff;
                                output[5] = output_value;
                                output_value = *(byte*)(buffer + (uVar16 + 6 & 0xfff));
                                *(byte*)(buffer + uVar12) = output_value;
                                uVar12 = uVar12 + 1 & 0xfff;
                                output[6] = output_value;
                                output_value = *(byte*)(buffer + (uVar16 + 7 & 0xfff));
                                *(byte*)(buffer + uVar12) = output_value;
                                uVar12 = uVar12 + 1 & 0xfff;
                                output[7] = output_value;
                                output = output + 8;
                                uloop_counter = uloop_counter - 1;
                            } while (uloop_counter != 0);
                        }
                        loop_counter = (int)(bottom + 3 - top);
                        if (top <= bottom + 2)
                        {
                            // write to.. buffer and output at same time?
                            do
                            {
                                bottom = encode_value + top;
                                top = top + 1;
                                first_value = *(byte*)(buffer + (bottom & 0xfff));
                                *(byte*)(buffer + uVar12) = first_value;
                                uVar12 = uVar12 + 1 & 0xfff;
                                *output = first_value;
                                output = output + 1;
                                loop_counter = loop_counter + -1;
                            } while (loop_counter != 0);
                        }
                    }
                    else
                    {
                        if (encode_value < 0x100)
                        {
                            first_value = *ptr_btlenemy;
                            encode_value = encode_value + 0x12;
                            ptr_btlenemy = new_ptr + 3;
                        }
                        else
                        {
                            encode_value = (top >> 8) + 2;
                        }
                        top = 0;
                        if (8 < encode_value + 1)
                        {
                            bottom = encode_value >> 3;
                            do
                            {
                                *(byte*)(buffer + uVar12) = first_value;
                                uVar12 = uVar12 + 1 & 0xfff;
                                top = top + 8;
                                *output = first_value;
                                *(byte*)(buffer + uVar12) = first_value;
                                uVar12 = uVar12 + 1 & 0xfff;
                                output[1] = first_value;
                                *(byte*)(buffer + uVar12) = first_value;
                                uVar12 = uVar12 + 1 & 0xfff;
                                output[2] = first_value;
                                *(byte*)(buffer + uVar12) = first_value;
                                uVar12 = uVar12 + 1 & 0xfff;
                                output[3] = first_value;
                                *(byte*)(buffer + uVar12) = first_value;
                                uVar12 = uVar12 + 1 & 0xfff;
                                output[4] = first_value;
                                *(byte*)(buffer + uVar12) = first_value;
                                uVar12 = uVar12 + 1 & 0xfff;
                                output[5] = first_value;
                                *(byte*)(buffer + uVar12) = first_value;
                                uVar12 = uVar12 + 1 & 0xfff;
                                output[6] = first_value;
                                *(byte*)(buffer + uVar12) = first_value;
                                uVar12 = uVar12 + 1 & 0xfff;
                                output[7] = first_value;
                                output = output + 8;
                                bottom = bottom - 1;
                            } while (bottom != 0);
                        }
                        loop_counter = (int)((encode_value + 1) - top);
                        if (top <= encode_value)
                        {
                            do
                            {
                                *(byte*)(buffer + uVar12) = first_value;
                                uVar12 = uVar12 + 1 & 0xfff;
                                *output = first_value;
                                output = output + 1;
                                loop_counter = loop_counter + -1;
                            } while (loop_counter != 0);
                        }
                    }
                }
                else
                {
                    // uvar1 ends in 1 we go here
                    first_value = *new_ptr;
                    ptr_btlenemy = new_ptr + 1;
                    *(byte*)(buffer + uVar12) = first_value;
                    uVar12 = uVar12 + 1 & 0xfff;
                    *output = first_value;
                    output = output + 1;
                }
            }
            return;
        }

    }
}
