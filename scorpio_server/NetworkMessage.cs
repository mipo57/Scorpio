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
        int msg_length = 0;
        bool is_raw_data = false;

        enum DataType { Void, AsciiString, UnicodeString, UInt32 }

        List<DataType> content_list;

        public bool ReciveHeader(byte[] msg, int msg_len)
        {
            if (msg_len > 10)
            {
                List<DataType> message_content = new List<DataType>();

                // message is not a header
                if (msg[0] != 1)
                    return false;

                // check if message is raw data
                if (msg[1] == 0)
                    is_raw_data = true;
                if (msg[1] == 1)
                    is_raw_data = false;

                if (!is_raw_data)
                {
                    // read data layout
                    for (int i = 2; i < 16; i++)
                    {
                        switch (msg[i])
                        {
                            case 0:
                                // Ignore
                                break;
                            case (int)DataType.AsciiString:
                                message_content.Add(DataType.AsciiString);
                                break;
                            case (int)DataType.UnicodeString:
                                message_content.Add(DataType.UnicodeString);
                                break;
                            case (int)DataType.UInt32:
                                message_content.Add(DataType.UInt32);
                                break;
                            default:
                                // unsuported format
                                return false;
                        }
                    }
                }

                // read total msg length
                msg_length = BitConverter.ToInt32(msg, 16);

                // sanity check (length cannot be less than zero or more than 1GB)
                if (msg_length < 0 || msg_length > 1073741824)
                    return false;

                return true;
            }

            return false;
        }

        public bool ReciveDataChunk(byte[] data, int data_len)
        {


            return true;
        }

        public bool AppendStringUnicode(string str)
        {
            byte[] msg_encoded = Encoding.Unicode.GetBytes(Constants.C_START_OF_TEXT + str + Constants.C_END_OF_TEXT);

            if (msg_length + msg_encoded.Length <= 1024)
            {
                msg_encoded.CopyTo(msg, msg_length);
                msg_length += msg_encoded.Length;

                return true;
            }

            return false;
        }

        public bool AppendStringASCII(string str)
        {
            byte[] msg_encoded = Encoding.ASCII.GetBytes(Constants.C_START_OF_TEXT + str + Constants.C_END_OF_TEXT);

            if (msg_length + msg_encoded.Length <= 1024)
            {
                msg_encoded.CopyTo(msg, msg_length);
                msg_length += msg_encoded.Length;

                return true;
            }

            return false;
        }

        public bool AppendUINT32(UInt32 value)
        {
            if (msg_length + 4 <= 1024)
            {
                BitConverter.GetBytes(value).CopyTo(msg, msg_length);
                msg_length += 4;

                return true;
            }

            return false;
        }

        public bool AppendFLOAT32(float value)
        {
            if (msg_length + 4 <= 1024)
            {
                BitConverter.GetBytes(value).CopyTo(msg, msg_length);
                msg_length += 4;

                return true;
            }

            return true;
        }
    }
}
