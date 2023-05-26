using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;
using System.Text;
using System;
using System.Threading;
using System.Runtime.Serialization;
using System.Xml.Linq;
using System.Xml;

public static class SaveSystem
{
    public static string filePath = Application.persistentDataPath;
    public static string fileName = "ASMRScraping.bin";
    public static string fullSavePath = Application.persistentDataPath + "/" + fileName;
    public static void SaveGameXML(SaveData saveData)
    {
        FileStream file = File.Create(fullSavePath);

        DataContractSerializer bf = new DataContractSerializer(saveData.GetType());
        MemoryStream streamer = new MemoryStream();

        //Serialize the file
        streamer.Seek(0, SeekOrigin.Begin);
        bf.WriteObject(streamer, saveData);
        char nul = (char)0;

        //bf.ReadObject()
        string s = System.Text.UTF8Encoding.UTF8.GetString(streamer.GetBuffer());    //doing this, because there were NUL chars at the end of the save file
        s = s.Split(nul)[0];


        string b64String = EncodeTo64(s);


        byte[] writeBuffer = System.Text.UTF8Encoding.UTF8.GetBytes(b64String);
        //byte[] writeBuffer = streamer.GetBuffer();
        // byte[] writeBuffer = System.Text.UTF8Encoding.UTF8.GetBytes(b64String);


        file.Write(writeBuffer, 0, writeBuffer.Length);
        file.Close();


    }

    public static void SaveGameAsyncXML(SaveData saveData)
    {
        Thread t = new Thread(() =>
        {
            FileStream file = File.Create(fullSavePath);

            DataContractSerializer bf = new DataContractSerializer(saveData.GetType());
            MemoryStream streamer = new MemoryStream();

            //Serialize the file
            streamer.Seek(0, SeekOrigin.Begin);
            bf.WriteObject(streamer, saveData);
            char nul = (char)0;

            //bf.ReadObject()
            string s = System.Text.UTF8Encoding.UTF8.GetString(streamer.GetBuffer());    //doing this, because there were NUL chars at the end of the save file
            s = s.Split(nul)[0];


            string b64String = EncodeTo64(s);


            byte[] writeBuffer = System.Text.UTF8Encoding.UTF8.GetBytes(b64String);
            //byte[] writeBuffer = streamer.GetBuffer();
            // byte[] writeBuffer = System.Text.UTF8Encoding.UTF8.GetBytes(b64String);


            file.Write(writeBuffer, 0, writeBuffer.Length);
            file.Close();
        });

        t.Start();

    }

    public static SaveData LoadGameXML()
    {
        if (File.Exists(fullSavePath))
        {
            // BinaryFormatter formatter = new BinaryFormatter();
            FileStream file = new FileStream(fullSavePath, FileMode.Open);

            MemoryStream streamer = new MemoryStream((int)file.Length);


            byte[] bytes = new byte[file.Length];

            file.Read(bytes, 0, (int)file.Length);

            //string b64String= DecodeFrom64(Convert.ToBase64String(bytes));
            string str = System.Text.UTF8Encoding.UTF8.GetString(bytes);


            string fromB64String = DecodeFrom64(str);

            //  string _byteOrderMarkUtf8 = Encoding.UTF8.GetString(Encoding.UTF8.GetPreamble());

            //   Debug.Log(fromB64String);
            byte[] readBytes = System.Text.UTF8Encoding.UTF8.GetBytes(fromB64String);
            // byte[] readBytes = System.Text.UTF8Encoding.UTF8.GetBytes(str);

            streamer.Write(readBytes, 0, readBytes.Length);

            DataContractSerializer bf = new DataContractSerializer(typeof(SaveData));

            streamer.Seek(0, SeekOrigin.Begin);


            SaveData saveData = null;

            saveData = (SaveData)bf.ReadObject(streamer);


            file.Close();

            return saveData;
        }
        else
        {
            Debug.LogError("Save file not found in " + fullSavePath);

            return null;
        }

    }

    static public string EncodeTo64(string toEncode)
    {
        byte[] toEncodeAsBytes
              = System.Text.ASCIIEncoding.ASCII.GetBytes(toEncode);
        string returnValue
              = System.Convert.ToBase64String(toEncodeAsBytes);
        return returnValue;
    }



    static public string DecodeFrom64(string encodedData)
    {
        byte[] encodedDataAsBytes
            = System.Convert.FromBase64String(encodedData);
        string returnValue =
           System.Text.ASCIIEncoding.ASCII.GetString(encodedDataAsBytes);
        return returnValue;
    }

    static public byte[] EncodeTo64(byte[] byteStringToEncode)
    {
        /*         byte[] toEncodeAsBytes
                      = System.Text.ASCIIEncoding.ASCII.GetBytes(toEncode); */
        string b64String
              = System.Convert.ToBase64String(byteStringToEncode);
        Debug.Log(b64String);
        byte[] returnValue = System.Text.ASCIIEncoding.ASCII.GetBytes(b64String);
        //byte[] returnValue = System.Convert.FromBase64String(b64String);
        return returnValue;
    }

    static public byte[] DecodeFrom64(byte[] byteStringToDecode)
    {
        string b64String = System.Convert.ToBase64String(byteStringToDecode);
        byte[] data = Convert.FromBase64String(b64String);
        string decodedString = System.Text.ASCIIEncoding.ASCII.GetString(data);
        Debug.Log(decodedString);
        byte[] returnValue = System.Text.ASCIIEncoding.ASCII.GetBytes(decodedString);

        return returnValue;
    }

    /*     public static void SaveGame(SaveData saveData)
        {
            Type[] types = { typeof(ProductInShop),typeof(CompletedContainerInfo)};
            var aSerializer = new XmlSerializer(typeof(SaveData), types);
            StringBuilder sb = new StringBuilder();


            FileStream stream = new FileStream(fullSavePath, FileMode.Create);
            aSerializer.Serialize(stream, saveData); // pass an instance of A      
            stream.Close();
        }



        public static SaveData LoadGame()
        {
            if (File.Exists(fullSavePath))
            {
                var aSerializer = new XmlSerializer(typeof(SaveData));
                FileStream stream = new FileStream(fullSavePath, FileMode.Open);

                SaveData saveData = aSerializer.Deserialize(stream) as SaveData;
                stream.Close();

                return saveData;
            }
            else
            {
                return null;
            }

        } */


}
