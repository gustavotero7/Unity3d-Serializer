//Chimera Digital - support@chimera.digital
//Gustavo Otero - gustavotero7@gmail.com, gustavo@chimera.digital

using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using UnityEngine;

/// <summary>
/// Herramienta para guardar y cargar informacion - Tool for save and load data
/// </summary>
public static class DataManager
{
    #region SaveToDataBase

    /// <summary>
    /// Carga datos encriptados desde la base de datos local, similar a la clase "Dyctionary" - Load encrypted data fro local data base, Like a dictyonary
    /// </summary>
    /// <param name="obgectType">Tipo de objeto a cargar - obgect type to load</param>
    /// <param name="key">clave a cargar - key to load</param>
    /// <returns>retorna los datos - retur the data</returns>
    public static object LoadData(Type obgectType, string key)
    {
        string deserializedData = PlayerPrefs.GetString(key);

        if (deserializedData == "") { 
            
            Debug.LogWarning("Not data available with key'" + key + "'");
            object _o = Activator.CreateInstance(obgectType);
            return _o; 
        
        }
        
        string decryptedData = Decrypt(deserializedData);

        object o = DeserializeFromString(obgectType, decryptedData);
        return o;
    }

    /// <summary>
    /// Guarda datos encriptados en la base de datos local, similar a la clase "Dyctionary" - Save encrypted data to local data base, Like a dictyonary
    /// </summary>
    /// <param name="obgectType">Tipo de objeto a guardar - obgect type to save</param>
    /// <param name="obgect">objeto que se va a guardar - obgect to save</param>
    /// <param name="key">clave a guardar - key to save</param>
    public static void SaveData(Type obgectType, object obgect, string key)
    {
        string serializedObject = SerializeToString(obgect);
        string encryptedObject = Encrypt(serializedObject);
        PlayerPrefs.SetString(key, encryptedObject);
    }
    #endregion

    #region SaveToFILE

    /// <summary>
    /// Carga datos encriptados de un archivo XML - Load encrypted data of the XML archive
    /// </summary>
    /// <param name="obgectType">Tipo de objeto a cargar - obgect type to load</param>
    /// <param name="fileName">archivo a cargar - file to load</param>
    /// <returns>retorna los datos - retur the data</returns>
    public static object LoadDataFile(Type obgectType, string fileName)
    {
        DecryptData(fileName);
        object o = Deserialize(obgectType, fileName);
        EncryptData(fileName);
        return o;
    }

    /// <summary>
    /// Guarda datos encriptados en un archivo XML - Save encrypted data in a XML file
    /// </summary>
    /// <param name="obgectType">Tipo de objeto a guardar - obgect type to save</param>
    /// <param name="obgect">objeto que se va a guardar - obgect to save</param>
    /// <param name="fileName">archivo a guardar - file to save</param>
    public static void SaveDataFile(Type obgectType, object obgect, string fileName)
    {
        Serialize(obgectType, obgect, fileName);
        EncryptData(fileName);

    }

#endregion

    

    /// <summary>
    /// Guarda datos en un archivo XML - Save data in a XML file
    /// </summary>
    /// <param name="obgectType">Tipo de objeto a guardar - obgect type to save</param>
    /// <param name="obgect">objeto que se va a guardar - obgect to save</param>
    /// <param name="fileName">archivo a guardar - file to save</param>
    public static void Serialize(Type obgectType, object obgect, string fileName)
    {
        XmlSerializer xmlSerializer = new XmlSerializer(obgectType);
        TextWriter textWritter = new StreamWriter(fileName);
        xmlSerializer.Serialize(textWritter, obgect);
        textWritter.Close();
    }


    public static string SerializeToString(object obj)
    {
        XmlSerializer serializer = new XmlSerializer(obj.GetType());

        using (StringWriter writer = new StringWriter())
        {
            serializer.Serialize(writer, obj);

            return Encrypt(writer.ToString());
        }
    }

    public static object DeserializeFromString(Type obgectType, string _toDeserialize)
    {
        string s = Decrypt(_toDeserialize);
        XmlSerializer xmlSerializer = new XmlSerializer(obgectType);
        TextReader textReader = new StringReader(s);
        object objectXml = xmlSerializer.Deserialize(textReader);
        textReader.Close();

        return objectXml;
    }
    
    /// <summary>
    /// Carga datos de un archivo XML - Load data of the XML archive
    /// </summary>
    /// <param name="obgectType">Tipo de objeto a cargar - obgect type to load</param>
    /// <param name="fileName">archivo a cargar - file to load</param>
    /// <returns>retorna los datos - retur the data</returns>
    public static object Deserialize(Type obgectType, string fileName)
    {
        if (!File.Exists(fileName))
            return null;

        XmlSerializer xmlSerializer = new XmlSerializer(obgectType);
        TextReader textReader = new StreamReader(fileName);
        object objectXml = xmlSerializer.Deserialize(textReader);
        textReader.Close();

        return objectXml;
    }


    /// <summary>
    /// Ecripta Un archivo XML - Encrypt a XML file
    /// </summary>
    /// <param name="fileName">archivo a encriptar - file to encrypt</param>
    public static void EncryptData(string fileName)
    {
        if (!File.Exists(fileName))
        {
            Debug.Log(fileName + "     NOT EXISTS");
            return;
        }

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(fileName);
            XmlElement elmRoot = xmlDoc.DocumentElement;
            string data = Encrypt(xmlDoc.InnerXml);
            elmRoot.RemoveAll();
            elmRoot.InnerText = data;
            xmlDoc.Save(fileName);
    }

    /// <summary>
    /// Desencripta Un archivo XML - Decrypt a XML file
    /// </summary>
    /// <param name="fileName">archivo a desencriptar - file to decrypt</param>
    public static void DecryptData(string fileName)
    {
        if (!File.Exists(fileName))
        {
            Debug.Log(fileName + "     NOT EXISTS");
            return;
        }

        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.Load(fileName);
        string data = Decrypt(xmlDoc.InnerText);
        xmlDoc.InnerXml = data;
        xmlDoc.Save(fileName);

    }


    public static string Encrypt(string toEncrypt)
    {

        byte[] keyArray = UTF8Encoding.UTF8.GetBytes("12345678901234567890123456789012");
        // 256-AES key
        byte[] toEncryptArray = UTF8Encoding.UTF8.GetBytes(toEncrypt);
        RijndaelManaged rDel = new RijndaelManaged();
        rDel.Key = keyArray;
        rDel.Mode = CipherMode.ECB;
        // http://msdn.microsoft.com/en-us/library/system.security.cryptography.ciphermode.aspx
        rDel.Padding = PaddingMode.PKCS7;
        // better lang support
        ICryptoTransform cTransform = rDel.CreateEncryptor();
        byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
        return Convert.ToBase64String(resultArray, 0, resultArray.Length);
    }

    public static string Decrypt(string toDecrypt)
    {
        byte[] keyArray = UTF8Encoding.UTF8.GetBytes("12345678901234567890123456789012");
        // AES-256 key
        byte[] toEncryptArray = Convert.FromBase64String(toDecrypt);
        RijndaelManaged rDel = new RijndaelManaged();
        rDel.Key = keyArray;
        rDel.Mode = CipherMode.ECB;
        // http://msdn.microsoft.com/en-us/library/system.security.cryptography.ciphermode.aspx
        rDel.Padding = PaddingMode.PKCS7;
        // better lang support
        ICryptoTransform cTransform = rDel.CreateDecryptor();
        byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
        return UTF8Encoding.UTF8.GetString(resultArray);
    }
}
