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
        int message_size = 0;
        int size_downloaded = 0;
        bool is_raw_data = false;

        enum DataType { Void, AsciiString, UnicodeString, UInt32 }

        List<DataType> content_list = new List<DataType>();
        List<Object> contents = new List<object>();
        byte[] data;

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
                message_size = BitConverter.ToInt32(msg, 16);

                // sanity check (length cannot be less than zero or more than 1GB)
                if (message_size < 0 || message_size > 1073741824)
                    return false;

                data = new byte[message_size];

                return true;
            }

            return false;
        }

        public bool ReciveDataChunk(byte[] bytes_recived, int data_len)
        {
            bytes_recived.Take(data_len).ToArray().CopyTo(data, size_downloaded);
            size_downloaded += data_len;

            if (size_downloaded > message_size)
                return false;

            return true;
        }

        public bool ComplieRecivedData()
        {
            if (size_downloaded < message_size)
                throw new Exception("Cannot compile partialy downloaded message!");

            // TODO: Write checksum checking

            int stack_pos = 0;
            for (int i = 0; i < content_list.Count; i++)
            {
                switch (content_list[i])
                {
                    case DataType.AsciiString:
                        if (data[stack_pos] == Constants.C_START_OF_TEXT)
                        {
                            int start_pos = stack_pos;
                            while (data[stack_pos] != Constants.C_END_OF_TEXT)
                            {
                                stack_pos++;

                                // prevent going beyond message size
                                if (stack_pos >= message_size)
                                    return false;
                            }

                            // get past the string
                            stack_pos++;

                            // get string without TEXT_START and TEXT_END markups
                            Encoding.ASCII.GetString(data, start_pos + 1, stack_pos - start_pos - 2);
                        }
                        else
                            return false;
                        break;
                    case DataType.UnicodeString:
                        if (data[stack_pos] == Constants.C_START_OF_TEXT)
                        {
                            int start_pos = stack_pos;
                            while (data[stack_pos] != Constants.C_END_OF_TEXT)
                            {
                                stack_pos++;

                                // prevent going beyond message size
                                if (stack_pos >= message_size)
                                    return false;
                            }

                            // get past the string
                            stack_pos++;

                            // get string without TEXT_START and TEXT_END markups
                            Encoding.ASCII.GetString(data, start_pos + 1, stack_pos - start_pos - 2);
                        }
                        else
                            return false;
                        break;
                    case DataType.UInt32:
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
