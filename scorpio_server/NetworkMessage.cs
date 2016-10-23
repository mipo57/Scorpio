using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Dynamic;
using Newtonsoft.Json;

namespace scorpio_server
{
    class NetworkMessage
    {
        Dictionary<string, byte[]> _data = new Dictionary<string, byte[]>();
        int _data_iterator = 0;

        static readonly byte[] header_magic_byte = new byte[4] { 0xAF, 0xFA, 0x0F, 0xA0 };

        public NetworkMessage() { }
        public NetworkMessage(byte[] data)
        {
            string json_string = Encoding.Unicode.GetString(data);

            try
            {
                _data = JsonConvert.DeserializeObject<Dictionary<string, byte[]>>(json_string);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception: {e.ToString()}");
            }
        }


        public void AttachString(string value, string key = null)
        {
            if (key == null)
            {
                key = _data_iterator.ToString();
                _data_iterator++;
            }

            _data[key] = Encoding.Unicode.GetBytes(value);
        }

        public void AttachUINT32(UInt32 value, string key = null)
        {
            if (key == null)
            {
                key = _data_iterator.ToString();
                _data_iterator++;
            }

            _data[key] = BitConverter.GetBytes(value);
        }

        public byte[] GetSerialized()
        {
            string data = JsonConvert.SerializeObject(_data);
            byte[] bin_data = Encoding.Unicode.GetBytes(data);

            // create header
            byte[] message = new byte[8];

            // header magic number
            message[0] = header_magic_byte[0];
            message[1] = header_magic_byte[1];
            message[2] = header_magic_byte[2];
            message[3] = header_magic_byte[3];

            // length of message
            BitConverter.GetBytes(bin_data.Length).CopyTo(message, 4);

            // add data to message
            message = message.Concat(bin_data).ToArray();

            return message;
        }

        public class HeaderInfo
        {
            public enum ReturnCode { HeaderFound, HeaderFoundPartially, HeaderNotFound}

            public ReturnCode return_code = ReturnCode.HeaderFound;
            public int data_start_index = -1;
            public int data_size = -1;
            public int bytes_saved = -1;

        }

        public uint GetElementUInt32 (string key)
        {
            byte[] value;

            if (_data.TryGetValue(key, out value))
            {
                return BitConverter.ToUInt32(value, 0);
            }
            else
            {
                Console.WriteLine($"Warning: Element {key} not found! Returning 0");
                return 0;
            }
                
        }

        public string GetElementString(string key)
        {
            byte[] value;

            if (_data.TryGetValue(key, out value))
            {
                return Encoding.Unicode.GetString(value);
            }
            else
            {
                Console.WriteLine($"Warning: Element {key} not found! Returning null");
                return null;
            }
        }

        public static HeaderInfo SearchForMessage(byte[] data)
        {
            HeaderInfo header_info = new HeaderInfo();

            // scan data stream for magic numbers
            int magic_byte_number = 0;
            int i = 0;
            for(; i < data.Length; i++)
            {
                if (data[i] == header_magic_byte[magic_byte_number])
                    magic_byte_number++;
                else
                    magic_byte_number = 0;

                if (magic_byte_number == 4)
                    break;
            }
            i++;

            // return message size if header magic number was found 
            if (magic_byte_number == 4 && i + 4 < data.Length)
            {
                header_info.data_size = BitConverter.ToInt32(data, i);
                header_info.return_code = HeaderInfo.ReturnCode.HeaderFound;
                header_info.data_start_index = i + 4;
            }
            else if (magic_byte_number > 0)
            {
                header_info.return_code = HeaderInfo.ReturnCode.HeaderFoundPartially;
                header_info.bytes_saved = magic_byte_number + data.Length - i + 1;
            }
            else
            {
                header_info.return_code = HeaderInfo.ReturnCode.HeaderNotFound;
            }

            return header_info;

        }
    }
}
