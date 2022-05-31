using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

class ConfigDecrypt
    {
    public static ulong getKey(ref string md5)
    {
        ulong key = Convert.ToUInt64(md5.Substring(0, 16), 16) ^ Convert.ToUInt64(md5.Substring(16, 16), 16);
        key = 0x995128D618026A71 * key;
        if (key < 0x42E77A425ACB90BB)
        {
            key = 0xffffffffffffffff - (0x42E77A425ACB90BB - key) + 1;
        }
        else
        {
            key = key - 0x42E77A425ACB90BB;
        }
        return key;
    }

    public static ulong bswap32(ulong a)
    {
        ulong result = (a & 0xff00ff00ff00ff00) >> 8 | (a & 0xff00ff00ff00ff) << 8;
        result = (result & 0xffff0000ffff0000) >> 0x10 | (result & 0xffff0000ffff) << 0x10;
        result = result >> 0x20 | result << 0x20;
        return result;
    }


    public static void decrypt(byte[] data, string file_name)
    {
        string md5 = file_name.Split('-')[1];
        ulong key = getKey(ref md5);
        ulong bswapped_key = bswap32(key);


        for (int i = 0; i < data.Length; i++)
        {
            int v41 = i & 7;

            ulong a = ((255UL << (8 * v41)) & bswapped_key) >> (8 * v41);

            data[i] ^= Convert.ToByte(a);

            if (v41 == 7)
            {
                key = (0x995128D618026A71 * key) & 0xffffffffffffffff;
                if (key < 0x42E77A425ACB90BB)
                    key = 0xffffffffffffffff - (0x42E77A425ACB90BB - key) + 1;
                else
                    key = key - 0x42E77A425ACB90BB;
                bswapped_key = bswap32(key);
            }

            data[0] = (byte)'{';
        }
    }
}
