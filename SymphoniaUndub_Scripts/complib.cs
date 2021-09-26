using System;
using System.Linq;

// Complib taken from https://github.com/AdmiralCurtiss/compto-sharp
// Thank you AdmiralCurtiss!!!

namespace SymphoniaUndub_Scripts
{
	class LzState
	{
		public int F;
		public int T;
		public ulong textsize;
		public ulong codesize;
		public ulong printcount;
		public byte[] text_buf = new byte[complib.N + complib.MF - 1];
		public int match_position;
		public int match_length;
		public int[] lson = new int[complib.N + 1];
		public int[] rson = new int[complib.N + 257];
		public int[] dad = new int[complib.N + 1];
	}

	public static class complib
	{
		public const int SUCCESS = 0;
		public const int ERROR_FILE_IN = -1;
		public const int ERROR_FILE_OUT = -2;
		public const int ERROR_MALLOC = -3;
		public const int ERROR_BAD_INPUT = -4;
		public const int ERROR_UNKNOWN_VERSION = -5;
		public const int ERROR_FILES_MISMATCH = -6;

		public const int N = 0x1000;
		public const int NIL = N;
		public const int MF = 0x12;
		public const int MAX_DUP = (0x100 + 0x12);

		private static System.IO.StreamWriter profilef = null;

		private static LzState LzStateCreate()
		{
			LzState State = new LzState();
			State.textsize = 0;
			State.codesize = 0;
			State.printcount = 0;
			return State;
		}

		private static void LzStateDelete(LzState State)
		{
			State = null;
		}

		private static string GetErrorString(int error)
		{
			switch (error)
			{
				case SUCCESS: return "Success";
				case ERROR_FILE_IN: return "Error with input file";
				case ERROR_FILE_OUT: return "Error with output file";
				case ERROR_MALLOC: return "Malloc failure";
				case ERROR_BAD_INPUT: return "Bad Input";
				case ERROR_UNKNOWN_VERSION: return "Unknown version";
				case ERROR_FILES_MISMATCH: return "Mismatch";
				default: return "Unknown error";
			}
		}

		private static void FillTextBuffer(LzState State)
		{
			int n, p;
			for (n = 0, p = 0; n != 0x100; n++, p += 8) { State.text_buf[p + 6] = State.text_buf[p + 4] = State.text_buf[p + 2] = State.text_buf[p + 0] = (byte)n; State.text_buf[p + 7] = State.text_buf[p + 5] = State.text_buf[p + 3] = State.text_buf[p + 1] = 0; }
			for (n = 0; n != 0x100; n++, p += 7) { State.text_buf[p + 6] = State.text_buf[p + 4] = State.text_buf[p + 2] = State.text_buf[p + 0] = (byte)n; State.text_buf[p + 5] = State.text_buf[p + 3] = State.text_buf[p + 1] = 0xff; }
			while (p != N) State.text_buf[p++] = 0;
		}

		private static int PrepareVersion(LzState State, int version)
		{
			if (State != null) State.T = 2;
			switch (version)
			{
				case 0: break;
				case 1: if (State != null) State.F = 0x12; break;
				case 3: if (State != null) State.F = 0x11; break;
				default: return ERROR_UNKNOWN_VERSION;
			}
			return SUCCESS;
		}

		private static void InitTree(LzState State)
		{
			int i;
			for (i = N + 1; i <= N + 256; i++) State.rson[i] = NIL;
			for (i = 0; i < N; i++) State.dad[i] = NIL;
		}

		private static void InsertNode(LzState State, int r)
		{
			int i, p, cmp;
			int key;

			cmp = 1; key = r; p = N + 1 + State.text_buf[key];
			State.rson[r] = State.lson[r] = NIL; State.match_length = 0;

			while (true)
			{
				if (cmp >= 0)
				{
					if (State.rson[p] != NIL) p = State.rson[p];
					else { State.rson[p] = r; State.dad[r] = p; return; }
				}
				else
				{
					if (State.lson[p] != NIL) p = State.lson[p];
					else { State.lson[p] = r; State.dad[r] = p; return; }
				}

				for (i = 1; i < State.F; i++) if ((cmp = State.text_buf[key + i] - State.text_buf[p + i]) != 0) break;

				if (i > State.match_length)
				{
					State.match_position = p;
					if ((State.match_length = i) >= State.F) break;
				}
			}

			State.dad[r] = State.dad[p]; State.lson[r] = State.lson[p]; State.rson[r] = State.rson[p];
			State.dad[State.lson[p]] = r; State.dad[State.rson[p]] = r;

			if (State.rson[State.dad[p]] == p) State.rson[State.dad[p]] = r; else State.lson[State.dad[p]] = r;

			State.dad[p] = NIL;
		}

		private static void DeleteNode(LzState State, int p)
		{
			int q;

			if (State.dad[p] == NIL) return;

			if (State.rson[p] == NIL) q = State.lson[p];
			else if (State.lson[p] == NIL) q = State.rson[p];
			else
			{
				q = State.lson[p];
				if (State.rson[q] != NIL)
				{
					do { q = State.rson[q]; } while (State.rson[q] != NIL);
					State.rson[State.dad[q]] = State.lson[q]; State.dad[State.lson[q]] = State.dad[q];
					State.lson[q] = State.lson[p]; State.dad[State.lson[p]] = q;
				}
				State.rson[q] = State.rson[p]; State.dad[State.rson[p]] = q;
			}
			State.dad[q] = State.dad[p];

			if (State.rson[State.dad[p]] == p) State.rson[State.dad[p]] = q; else State.lson[State.dad[p]] = q;
			State.dad[p] = NIL;
		}

		public static int Encode(int version, byte[] @in, int inl, byte[] @out, ref uint outl)
		{
			/* pointers! */
			int insp = 0, inst = 0, ousp = 0, oust = 0, inspb = 0, insplb = 0;
			int i, c, len, r, s, last_match_length, dup_match_length = 0, code_buf_ptr, dup_last_match_length = 0;
			byte[] code_buf = new byte[1 + 8 * 5];
			byte mask;
			int error = SUCCESS;
			LzState State = LzStateCreate();
			if (State == null) goto _cleanup;

			inst = inl; oust = (int)(outl);

			if (version == 0)
			{
				while (true)
				{
					int left = inst - insp;
					int left8 = left;
					//printf("left8:%d\n", left8);
					if (left8 > 8) left8 = 8;
					//{ if (ousp >= oust) { error = SUCCESS; goto _cleanup; } @out[ousp++] = (byte)(((1 << left8) - 1) << (8 - left8)); }
					{ if (ousp >= oust) { error = SUCCESS; goto _cleanup; } @out[ousp++] = (byte)(((1 << left8) - 1) << (0)); }

					for (i = 0; i < 8; i++)
					{
						if (insp >= inst) break; c = @in[insp++];
						{ if (ousp >= oust) { error = SUCCESS; goto _cleanup; } @out[ousp++] = (byte)c; }
					}

					if (insp >= inst) break;
				}

				if (insp != inst)
				{
					LzStateDelete(State);
					Console.WriteLine("(insp != inst) ({0} != {1})", insp, inst);
					return ERROR_BAD_INPUT;
				}

				outl = (uint)(ousp - 0);

				LzStateDelete(State);
				return error;
			}

			FillTextBuffer(State);
			PrepareVersion(State, version);
			InitTree(State);

			code_buf[0] = 0x00;
			code_buf_ptr = mask = 1;
			s = 0; r = N - State.F;

			//printf("%d\n", r);

			for (len = 0; len < State.F; len++) { if (insp >= inst) break; c = @in[insp++]; State.text_buf[r + len] = (byte)c; }
			if ((State.textsize = (ulong)len) == 0) return SUCCESS;

			for (i = 1; i <= State.F; i++) InsertNode(State, r - i);
			InsertNode(State, r);

			do
			{
				if (version >= 3)
				{
					if (insplb - inspb <= 0)
					{
						insplb = inspb + 1;
						while ((insplb < inst) && (@in[insplb] == @in[inspb])) insplb++;
					}

					dup_match_length = insplb - inspb;
				}

				if (State.match_length > len) State.match_length = len;

				if (version >= 3 && dup_match_length > MAX_DUP) dup_match_length = MAX_DUP;

				if (version >= 3 && dup_match_length > (State.T + 1) && dup_match_length >= State.match_length)
				{
					if (dup_match_length >= (inst - insp)) dup_match_length--;
				}
				else
				{
					if (State.match_length >= (inst - insp)) State.match_length--;
				}
				/*
				if (version >= 3 && dup_match_length > (State.T + 1) && dup_match_length >= State.match_length) {
					if (dup_match_length >= (inst - insp)) dup_match_length -= 8;
				} else {
					if (State.match_length >= (inst - insp)) State.match_length -= 8;
				}
				*/

				if (version >= 3 && dup_match_length > (State.T + 1) && dup_match_length >= State.match_length)
				{
					State.match_length = dup_match_length;
					State.match_position = r;

					if (State.match_length <= 0x12)
					{
						code_buf[code_buf_ptr++] = State.text_buf[r];
						code_buf[code_buf_ptr++] = (byte)(0x0f | (((State.match_length - (State.T + 1)) & 0xf) << 4));
					}
					else
					{
						code_buf[code_buf_ptr++] = (byte)(State.match_length - 0x13);
						code_buf[code_buf_ptr++] = 0x0f;
						code_buf[code_buf_ptr++] = State.text_buf[r];
					}
				}
				else if (State.match_length > State.T)
				{
					code_buf[code_buf_ptr++] = (byte)State.match_position;
					code_buf[code_buf_ptr++] = (byte)(((State.match_position >> 4) & 0xf0) | ((State.match_length - (State.T + 1)) & 0x0f));
				}
				else
				{
					code_buf[0] |= mask;
					State.match_length = 1;
					code_buf[code_buf_ptr++] = State.text_buf[r];
				}

				if ((mask <<= 1) == 0)
				{
					for (i = 0; i < code_buf_ptr; i++) { if (ousp >= oust) { error = SUCCESS; goto _cleanup; } @out[ousp++] = code_buf[i]; }
					State.codesize += (ulong)code_buf_ptr;
					code_buf[0] = 0x00; code_buf_ptr = mask = 1;
				}

				last_match_length = State.match_length;
				for (i = 0; i < last_match_length; i++)
				{
					if (insp >= inst) break; c = @in[insp++]; DeleteNode(State, s); State.text_buf[s] = (byte)c;
					if (s < State.F - 1) State.text_buf[s + N] = (byte)c;
					s = (s + 1) & (N - 1); r = (r + 1) & (N - 1);
					inspb++;
					InsertNode(State, r);
				}

				State.textsize += (ulong)i;

				while (i++ < last_match_length)
				{
					DeleteNode(State, s); s = (s + 1) & (N - 1); r = (r + 1) & (N - 1);
					inspb++;
					if ((--len) != 0) InsertNode(State, r);
				}
			} while (len > 0);

			if (code_buf_ptr > 1)
			{
				for (i = 0; i < code_buf_ptr; i++) { if (ousp >= oust) { error = SUCCESS; goto _cleanup; } @out[ousp++] = code_buf[i]; }
				State.codesize += (ulong)code_buf_ptr;
			}

		_cleanup:

			if (State == null) return ERROR_MALLOC;
			if (insp != inst)
			{
				Console.WriteLine("(insp != inst) ({0} != {1})", insp, inst);
				return ERROR_BAD_INPUT;
			}

			outl = (uint)(ousp - 0);
			LzStateDelete(State);

			return SUCCESS;
		}

		public static int Decode(int version, byte[] @in, uint inl, byte[] @out, ref uint outl)
		{
			if (version == 0 && inl == outl)
			{
				for (uint loopidx = 0; loopidx < inl; ++loopidx)
				{
					@out[loopidx] = @in[loopidx];
				}
				return SUCCESS;
			}

			/* pointers! */
			int insp = 0, inst = 0, ousp = 0, oust = 0;
			uint flags = 0, i, j, k, r, c;
			int error = SUCCESS;
			LzState State = LzStateCreate();
			if (State == null) goto _cleanup;

			inst = (int)inl; oust = (int)(outl);

			FillTextBuffer(State);
			if ((error = PrepareVersion(State, version)) != SUCCESS) return error;
			r = (uint)(N - State.F);

			for (; ; )
			{
				if (((flags >>= 1) & 0x100) == 0) { if (insp >= inst) break; c = @in[insp++]; if (profilef != null) profilef.WriteLine("-------- {0:X2} -------- [{1:X8}:{2:X8}]", c, insp - 0, ousp - 0); flags = c | 0xff00; }
				if ((flags & 1) != 0) { if (insp >= inst) break; c = @in[insp++]; if (profilef != null) profilef.WriteLine("BYTE[{0:X2}]", c); { { if (ousp >= oust) { error = SUCCESS; goto _cleanup; } @out[ousp++] = (byte)c; } State.text_buf[r++] = (byte)c; r &= (uint)(N - 1); }; continue; }
				if (insp >= inst) break; i = @in[insp++]; if (insp >= inst) break; j = @in[insp++]; i |= (j & 0xf0) << 4; j = (uint)((j & 0x0f) + State.T);
				if (version == 1 || j < (State.F)) { if (profilef != null) profilef.Write("WINDOW[{0:X3},*{1:X2}] : (", j + 1, i); for (k = 0; k <= j; k++) { c = State.text_buf[(i + k) & (N - 1)]; { { if (ousp >= oust) { error = SUCCESS; goto _cleanup; } @out[ousp++] = (byte)c; } State.text_buf[r++] = (byte)c; r &= (uint)(N - 1); }; if (profilef != null && k != 0) profilef.Write(", "); if (profilef != null) profilef.Write("{0:X2}", c); } if (profilef != null) profilef.WriteLine(")"); continue; }
				if (i < 0x100) { if (insp >= inst) break; j = @in[insp++]; i += (uint)(State.F + 1); } else { j = i & 0xff; i = (uint)((i >> 8) + State.T); }
				if (profilef != null) profilef.Write("REPEAT[{0:X3},{1:X2}] : (", i + 1, j); for (k = 0; k <= i; k++) { { { if (ousp >= oust) { error = SUCCESS; goto _cleanup; } @out[ousp++] = (byte)j; } State.text_buf[r++] = (byte)j; r &= (uint)(N - 1); }; if (profilef != null && k != 0) profilef.Write(", "); if (profilef != null) profilef.Write("{0:X2}", j); }
				if (profilef != null) profilef.WriteLine(")");
			}

		_cleanup:

			if (State == null) return ERROR_MALLOC;
			if (insp != inst) return ERROR_BAD_INPUT;

			outl = (uint)ousp;
			LzStateDelete(State);

			return error;
		}

		private static uint ReadUInt(System.IO.Stream s, bool littleEndian)
		{
			int b1 = s.ReadByte();
			int b2 = s.ReadByte();
			int b3 = s.ReadByte();
			int b4 = s.ReadByte();
			if (littleEndian)
			{
				return (uint)(b4 << 24 | b3 << 16 | b2 << 8 | b1);
			}
			else
			{
				return (uint)(b1 << 24 | b2 << 16 | b3 << 8 | b4);
			}
		}

		public static int DecodeStream(System.IO.Stream fin, System.IO.Stream fout, int raw, int version, bool littleEndian)
		{
			int error = SUCCESS;
			uint inl, outl;
			byte[] ind, outd;

			if (raw != 0)
			{
				inl = (uint)fin.Length;
				outl = inl * 10;
			}
			else
			{
				version = fin.ReadByte();
				inl = ReadUInt(fin, littleEndian);
				outl = ReadUInt(fin, littleEndian);
				if (PrepareVersion(null, version) != SUCCESS) { error = ERROR_FILE_IN; goto _cleanup; }
			}

			ind = new byte[inl];
			outd = new byte[outl];

			fin.Read(ind, 0, (int)inl);

			error = Decode(version, ind, inl, outd, ref outl);

			if (fout != null)
			{
				fout.Write(outd, 0, (int)outl);
			}

		_cleanup:

			outd = null;
			ind = null;

			return error;
		}

		public static int DecodeFile(string @in, string @out, int raw, int version, bool littleEndian)
		{
			int error = SUCCESS;
			System.IO.FileStream fin, fout = null;

			Console.Write("Decoding[{0:X2}] {1} -> {2}...", version, @in ?? "", @out ?? "");

			fin = new System.IO.FileStream(@in, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.Read);

			if (@out != null)
			{
				fout = new System.IO.FileStream(@out, System.IO.FileMode.Create);
			}

			error = DecodeStream(fin, fout, raw, version, littleEndian);

			if (fout != null) fout.Close();
			if (fin != null) fin.Close();

			Console.WriteLine(GetErrorString(error));

			return error;
		}

		private static void WriteUInt(System.IO.Stream s, uint v, bool littleEndian)
		{
			byte b1 = (byte)(v & 0xFF);
			byte b2 = (byte)((v >> 8) & 0xFF);
			byte b3 = (byte)((v >> 16) & 0xFF);
			byte b4 = (byte)((v >> 24) & 0xFF);
			if (littleEndian)
			{
				s.WriteByte(b1);
				s.WriteByte(b2);
				s.WriteByte(b3);
				s.WriteByte(b4);
			}
			else
			{
				s.WriteByte(b4);
				s.WriteByte(b3);
				s.WriteByte(b2);
				s.WriteByte(b1);
			}
		}

		public static int EncodeStream(System.IO.Stream fin, System.IO.Stream fout, int raw, int version, bool littleEndian)
		{
			int error = SUCCESS;
			uint inl, outl;
			byte[] ind, outd;

			int eversion = 0;

			if (version < 0)
			{
				version = -version;
				eversion = 0;
			}
			else
			{
				eversion = version;
			}

			//Console.WriteLine("{0}, {1}", version, eversion);

			inl = (uint)fin.Length; outl = ((inl * 9) / 8) + 10;

			ind = new byte[inl];
			outd = new byte[outl];

			fin.Read(ind, 0, (int)inl);

			error = Encode(eversion, ind, (int)inl, outd, ref outl);

			if (fout != null)
			{
				if (raw == 0)
				{
					fout.WriteByte((byte)version);
					WriteUInt(fout, outl, littleEndian);
					WriteUInt(fout, inl, littleEndian);
				}

				fout.Write(outd, 0, (int)outl);
			}

			outd = null;
			ind = null;

			Console.WriteLine(GetErrorString(error));

			return error;
		}

		public static int EncodeFile(string @in, string @out, int raw, int version, bool littleEndian)
		{
			int error = SUCCESS;
			System.IO.FileStream fin = null, fout = null;
			Console.Write("Encoding[{0:X2}] {1} -> {2}...", version < 0 ? -version : version, @in ?? "", @out ?? "");

			fin = new System.IO.FileStream(@in, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.Read);

			if (@out != null)
			{
				fout = new System.IO.FileStream(@out, System.IO.FileMode.Create);
			}

			error = EncodeStream(fin, fout, raw, version, littleEndian);

			if (fout != null) fout.Close();
			if (fin != null) fin.Close();

			return error;
		}

		public static int DumpTextBuffer(string @out)
		{
			int error = SUCCESS;
			System.IO.FileStream fout = null;

			Console.Write("Dumping text buffer...");

			LzState State = LzStateCreate();
			if (State == null) goto _cleanup;

			FillTextBuffer(State);

			fout = new System.IO.FileStream(@out, System.IO.FileMode.Create);

			fout.Write(State.text_buf, 0, N);

		_cleanup:

			if (State != null) LzStateDelete(State);
			if (fout != null) fout.Close();

			Console.WriteLine(GetErrorString(error));

			return error;
		}

		public static int CheckCompression(string @in, int version)
		{
			System.IO.FileStream fin = null; byte[] ind = null, outd = null, outd2 = null;
			int error = SUCCESS; uint inl, outl, outl2;

			Console.Write("Checking compression [{0:X2}] ({1}) ...", version, @in);

			fin = new System.IO.FileStream(@in, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.Read);

			outl2 = inl = (uint)fin.Length; outl = ((inl * 9) / 8) + 10;

			ind = new byte[inl];
			outd = new byte[outl];
			outd2 = new byte[outl2];

			fin.Read(ind, 0, (int)inl);

			if ((error = Encode(version, ind, (int)inl, outd, ref outl)) != SUCCESS) { goto _cleanup; }

			if ((error = Decode(version, outd, outl, outd2, ref outl2)) != SUCCESS) { goto _cleanup; }

			if (inl != outl2) { error = ERROR_FILES_MISMATCH; goto _cleanup; }
			if (!ind.SequenceEqual(outd2)) { error = ERROR_FILES_MISMATCH; goto _cleanup; }

		_cleanup:

			outd2 = null;
			outd = null;
			ind = null;

			if (fin != null) fin.Close();

			Console.WriteLine(GetErrorString(error));

			return error;
		}

		public static void ProfileStart(string @out)
		{
			profilef = new System.IO.StreamWriter(@out, false);
		}

		public static void ProfileEnd()
		{
			if (profilef == null) return;
			profilef.Close();
			profilef = null;
		}
	}
}
