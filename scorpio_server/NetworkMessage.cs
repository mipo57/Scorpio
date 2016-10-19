using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;

namespace scorpio_server
{
    class NetworkMessage
    {
        byte[] msg = new byte[1024];
        int stack_pos = 0;

        public bool AppendStringUnicode(string str)
        {
            byte[] msg_encoded = Encoding.Unicode.GetBytes(Constants.C_START_OF_TEXT + str + Constants.C_END_OF_TEXT);

            if (stack_pos + msg_encoded.Length <= 1024)
            {
                msg_encoded.CopyTo(msg, stack_pos);
                stack_pos += msg_encoded.Length;

                return true;
            }

            return false;
        }

        public bool AppendStringASCII(string str)
        {
            byte[] msg_encoded = Encoding.ASCII.GetBytes(Constants.C_START_OF_TEXT + str + Constants.C_END_OF_TEXT);

            if (stack_pos + msg_encoded.Length <= 1024)
            {
                msg_encoded.CopyTo(msg, stack_pos);
                stack_pos += msg_encoded.Length;

                return true;
            }

            return false;
        }

        public bool AppendUINT32(UInt32 value)
        {
            if (stack_pos + 4 <= 1024)
            {
                BitConverter.GetBytes(value).CopyTo(msg, stack_pos);
                stack_pos += 4;

                return true;
            }

            return false;
        }

        public bool AppendFLOAT32(float value)
        {
            if (stack_pos + 4 <= 1024)
            {
                BitConverter.GetBytes(value).CopyTo(msg, stack_pos);
                stack_pos += 4;

                return true;
            }

            return true;
        }
    }
}
