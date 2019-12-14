using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace CountURLWordFrequency
{
    public partial class Default : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            LoadSavedWords();
        }
        /// <summary>
        /// Counts frequence of all words in from URL.
        /// </summary>
        /// <param name="URL">The URL of the page to count the words in.</param>
        /// <remarks>The output will be in descending order of word frequency. Words are counted
        /// without regard to case-sensitivity, and punctuation marks are ignored.

        /// </remarks>
        /// 

        protected void ButtonCount_Click(object sender, EventArgs e)
        {
          
            try
            {
          
                    bool valid = true;
                    String PageURL = Request.Form["PageURL"];               
                    if (PageURL == "")
                    {
                    PageURLDescriptionPlaceHolder.Controls.Add(new Literal { Text = "Required" });
                     valid = false;
                    }
                    else
                    {
                    PageURLDescriptionPlaceHolder.Controls.Add(new Literal { Text = "" });
                    }

                   
                    if (!valid)
                    {
                        return;
                }
                else
                {
                    CountWordFrequency(PageURL); ;
             
                }
                


            }
            catch (Exception exp)
            {
                ErrorPlaceHolder.Controls.Add(new Literal { Text =exp.Message });    
            }         
            return;


        }


        public void CountWordFrequency(string URL)
        {
     

            Dictionary<string, int> wordCounts = new Dictionary<string, int>();
            string pattern = @"[^a-zA-Z0-9'\- ]";

         

            using (WebClient client = new WebClient())
            {
                //get the page source
                string html = client.DownloadString(URL).ToLower();

                //remove html elements
                html = RemoveHtmlTags(html);
                string cleanedLine = Regex.Replace(html, pattern, string.Empty).ToLowerInvariant();
                string[] words = cleanedLine.Split(' ');
                foreach (string word in words)
                {
                    // Ignore empty tokens.
                    if (!string.IsNullOrEmpty(word))
                    {
                        int frequency = 1;
                        if (wordCounts.ContainsKey(word))
                        {
                            frequency = wordCounts[word] + 1;
                        }

                        // N.B. If there is no key for the current word,
                        // an item is added to the dictionary for that key.
                        wordCounts[word] = frequency;
                    }
                }

                // Since Dictionary<K, V> implements IEnumerable<KeyValuePair<K, V>>,
                // we can simply initialize the list using the constructor. Otherwise,
                // we could also use a foreach loop on the dictionary, adding each pair
                // to the list manually.
                List<KeyValuePair<string, int>> pairList = new List<KeyValuePair<string, int>>(wordCounts);

                // One overload of the Sort() method takes a Comparison<T> delegate.
                // That is, a function that takes two arguments of type T and returns
                // an int value indicating the relative values of the arguments. In
                // this case, we are using an anonymous method to accomplish this.
                pairList.Sort((first, second) => { return second.Value.CompareTo(first.Value); });
                int counter = 1;
                int maxfontsize = 300;
                foreach (KeyValuePair<string, int> pair in pairList)
                {
                    if (counter <= 100) {
                        WordsPlaceHolder.Controls.Add(new Literal { Text = " <li class='list-group-item' style='font-size: "+ maxfontsize + "%'>" + pair.Key + " : " + pair.Value + "</li>" });
                    }
                    else
                    {
                        break;
                    }
                    counter++;
                    maxfontsize = maxfontsize - 2;
                }
                SaveWordResult(pairList);


            }


        }

      
            public void SaveWordResult(List<KeyValuePair<string, int>> pairList)
        {
            var dbCon = DBConnection.Instance();
            if (dbCon.IsConnect())
            {
                UnicodeEncoding ByteConverter = new UnicodeEncoding();
                int counter = 1;
                foreach (KeyValuePair<string, int> pair in pairList)
                {
                    if (counter<=100)
                    {
                        //   WordsPlaceHolder.Controls.Add(new Literal { Text = " <li class='list-group-item' style='font-size: " + maxfontsize + "%'>" + pair.Key + " : " + pair.Value + "</li>" });
                        byte[] plaintext;

                        byte[] SaltedHash = GenerateSaltedHash(ByteConverter.GetBytes(pair.Key));

                        //suppose col0 and col1 are defined as VARCHAR in the DB
                        string query = "SELECT * FROM words WHERE wordsaltedhash=@SaltedHash";
                        var cmd = new MySqlCommand(query, dbCon.Connection);
                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.AddWithValue("@SaltedHash", SaltedHash);

                        var reader = cmd.ExecuteReader();
                        if (reader.HasRows)
                        {
                            reader.Close();

                            string query2 = "UPDATE words SET totalcount=@totalcount WHERE wordsaltedhash=@wordsaltedhash)";
                            var insertcmd = new MySqlCommand(query2, dbCon.Connection);
                            insertcmd.CommandType = CommandType.Text;
                            insertcmd.Parameters.AddWithValue("@wordsaltedhash", SaltedHash);
                            insertcmd.Parameters.AddWithValue("@totalcount", pair.Value);
                            insertcmd.ExecuteNonQuery();
                            //update
                        }
                        else
                        {
                           
                            RSACryptoServiceProvider RSA = new RSACryptoServiceProvider();
                            plaintext = ByteConverter.GetBytes(pair.Key);
                            byte[] encryptedtext;
                            encryptedtext = AsymetricEncrypt(plaintext, RSA.ExportParameters(false), false);

                            reader.Close();
                            string query2 = "INSERT INTO words(wordsaltedhash,wordasymetricencription,totalcount,wordtxt)VALUES(@wordsaltedhash,@wordasymetricencription,@totalcount,@wordtxt)";
                            var insertcmd = new MySqlCommand(query2, dbCon.Connection);
                            insertcmd.CommandType = CommandType.Text;
                            insertcmd.Parameters.AddWithValue("@wordsaltedhash", SaltedHash);
                            insertcmd.Parameters.AddWithValue("@wordasymetricencription", encryptedtext);
                            insertcmd.Parameters.AddWithValue("@totalcount", pair.Value);
                            insertcmd.Parameters.AddWithValue("@wordtxt", pair.Key);
                            insertcmd.ExecuteNonQuery();
                        }
                    }
                    else
                    {
                        break;
                    }
                    counter++;
                  

                } 
               
                ErrorPlaceHolder.Controls.Add(new Literal { Text = counter-1+" Words Saved Successfully" });

            }
            else
            {
                ErrorPlaceHolder.Controls.Add(new Literal { Text = "Words generated but failed to connect to MYSQL Database, Update the Connection String and try again" });
            }
        }

        public void LoadSavedWords()
        {
            try
            {

                var dbCon = DBConnection.Instance();
                if (dbCon.IsConnect())
                {
                    UnicodeEncoding ByteConverter = new UnicodeEncoding();
                    RSACryptoServiceProvider RSA = new RSACryptoServiceProvider();

                    try
                    {

                        //suppose col0 and col1 are defined as VARCHAR in the DB
                        string q = "CREATE TABLE `words` (`wordsaltedhash` varbinary(767) NOT NULL,`wordasymetricencription` varbinary(767) DEFAULT NULL,`totalcount` int(11) DEFAULT NULL,`wordtxt` varchar(500) DEFAULT NULL,PRIMARY KEY (`wordsaltedhash`)) ENGINE=InnoDB DEFAULT CHARSET=latin1";
                        var c = new MySqlCommand(q, dbCon.Connection);
                        c.CommandType = CommandType.Text;
                       c.ExecuteNonQuery();
                    }
                    catch
                    {

                    }

                    //suppose col0 and col1 are defined as VARCHAR in the DB
                    string query = "SELECT wordasymetricencription,totalcount,wordtxt FROM words ORDER BY totalcount DESC";
                    var cmd = new MySqlCommand(query, dbCon.Connection);
                    cmd.CommandType = CommandType.Text;                   
                    var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        String totalcount = "";
                        String txt = "";
                        try
                        {
                            totalcount = reader["totalcount"].ToString();
                            byte[] wordasymetricencription = (byte[])reader["wordasymetricencription"];
                            txt = reader["wordtxt"].ToString();
                            byte[] decryptedtex = AsymetricDecrypt(wordasymetricencription, RSA.ExportParameters(true), false);
                           // txt = ByteConverter.GetString(decryptedtex);
                          
                        }
                        catch (Exception exp)
                        {

                        }
                        SavedWordsPlaceHolder.Controls.Add(new Literal { Text = "<tr><td>" + txt + "</td><td>" + totalcount + "</td></tr>" });





                    }
                    reader.Close();
           
                }
                else
                {
                    ErrorPlaceHolder2.Controls.Add(new Literal { Text = "Failed to connect to MYSQL Database, Update the Connection String and try again" });
                }
            }
            catch (Exception exp)
            {
                ErrorPlaceHolder2.Controls.Add(new Literal { Text = exp.Message});

            }

        }

        public  byte[] GenerateSaltedHash(byte[] plainText)
        {
       
            using (HashAlgorithm algorithm = new SHA256Managed())
            {
                byte[] salt = GenerateSalt();
                byte[] saltedText = new byte[plainText.Length + salt.Length];

                plainText.CopyTo(saltedText, 0);
                salt.CopyTo(saltedText, plainText.Length);

                return algorithm.ComputeHash(saltedText);
            }
        }

        private  byte[] GenerateSalt()
        {
            using (RandomNumberGenerator random = new RNGCryptoServiceProvider())
            {
                byte[] salt = new byte[32];
                random.GetNonZeroBytes(salt);
                return salt;
            }
        }

         public byte[] AsymetricEncrypt(byte[] byteEncrypt, RSAParameters RSAInfo, bool isOAEP)
        {
            try
            {
                byte[] encryptedData;
                //Create a new instance of RSACryptoServiceProvider.
                using (RSACryptoServiceProvider RSA = new RSACryptoServiceProvider())
                {
                    //Import the RSA Key information. This only needs
                    //toinclude the public key information.
                    RSA.ImportParameters(RSAInfo);

                    //Encrypt the passed byte array and specify OAEP padding.
                    encryptedData = RSA.Encrypt(byteEncrypt, isOAEP);
                }
                return encryptedData;
            }
            //Catch and display a CryptographicException
            //to the console.
            catch (CryptographicException e)
            {
                Console.WriteLine(e.Message);

                return null;
            }
        }

    
         public byte[] AsymetricDecrypt(byte[] byteDecrypt, RSAParameters RSAInfo, bool isOAEP)
        {
            try
            {
                byte[] decryptedData;
                //Create a new instance of RSACryptoServiceProvider.
                using (RSACryptoServiceProvider RSA = new RSACryptoServiceProvider())
                {
                    //Import the RSA Key information. This needs
                    //to include the private key information.
                    RSA.ImportParameters(RSAInfo);

                    //Decrypt the passed byte array and specify OAEP padding.
                    decryptedData = RSA.Decrypt(byteDecrypt, isOAEP);
                }
                return decryptedData;
            }
            //Catch and display a CryptographicException
            //to the console.
            catch (CryptographicException e)
            {
           
               // ErrorPlaceHolder2.Controls.Add(new Literal { Text = e.Message });
                return null;
            }
        }
            //Removes Html Tags from input provided
            public  string RemoveHtmlTags(string html)
        {
            string htmlRemoved = Regex.Replace(html, @"<script[^>]*>[\s\S]*?</script>|<[^>]+>| ", " ").Trim();
            string normalised = Regex.Replace(htmlRemoved, @"\s{2,}", " ");
            return normalised;
        }

        /// <summary>
        /// Finds the file given the file name.
        /// </summary>
        /// <param name="fileName">The name of the file to find.</param>
        /// <returns>The full path to the file.</returns>
        /// <remarks>The FindFile method is a no-op if the fileName
        /// argument represents a full path to a valid file. Otherwise,
        /// it looks in the same directory as the executing assembly.</remarks>
        /// 
   

        /// <summary>
        /// Removes unwanted characters from a given line of text.
        /// </summary>
        /// <param name="line">The line to remove the characters from.</param>
        /// <returns>The modified line of text with all unwanted characters removed,
        /// and normalized to all lower case.</returns>
        /// <remarks>The CleanLine method uses a regular expression to 
        /// remove characters from the line. This gives us a single place
        /// to modify if the list of characters to remove changes.</remarks>
        private static string CleanLine(string line)
        {
            // Removing everything except ASCII alphanumerics, hyphens
            // (for hyphenated words), and apostrophes (for contractions).
            // N.B. This doesn't handle the case of embedded quotes within
            // text like the following:
            // John said, "He told me, 'I want to see you.'"
            // Furthermore, punctuation without a space will be collapsed
            // (e.g., "hello,world" will be rendered as "helloworld").
            string pattern = @"[^a-zA-Z0-9'\- ]";
            string cleanedLine = Regex.Replace(line, pattern, string.Empty);
            return cleanedLine.ToLowerInvariant();
        }

        /// <summary>
        /// Compares two KeyValuePair{string, int} objects by value.
        /// </summary>
        /// <param name="first">The KeyValuePair{string, int} to compare.</param>
        /// <param name="second">The KeyValuePair{string, int} to compare to.</param>
        /// <returns>An integer value indicating the relative value of the 
        /// KeyValuePair{string, int} objects. </returns>
        /// <remarks>A negative value means the first pair has a lower number
        /// in its Value property than the second. A positive value means the
        /// first pair has a higher value in its Value property than the second.
        /// A zero value means that the Value properties of the two pairs are
        /// equal.</remarks>
        private static int ComparePairs(KeyValuePair<string, int> first, KeyValuePair<string, int> second)
        {
            // The CompareTo() returns an int indicating relative value of
            // the two objects. It returns a negative value if the current
            // instance is before the operand in the sort order, returns a 
            // positive value if the current instance is after the operand
            // in the sort order, and zero if they are equal. We compare
            // the second to the first, because we want a descending sort.
            // Though the spec doesn't specify, we could define a behavior
            // for sorting if the values are equal here (e.g., sort equally
            // frequent words alphabetically).
            return second.Value.CompareTo(first.Value);
        }

        /// <summary>
        /// Gets the format string for output based on the longest word 
        /// length and highest frequency.
        /// </summary>
        /// <param name="longestWordLength">The length of the longest word.</param>
        /// <param name="highestFrequency">The highest occuring frequency.</param>
        /// <returns>A format string for printing each word and frequency.</returns>
        private static string GetFormatString(int longestWordLength, int highestFrequency)
        {
            // Get the number of digits by repeatedly dividing by 10.
            // Assigning to an int will discard the decimal portion,
            // acting as a "div" operator.
            int frequencyDigits = 0;
            while (highestFrequency > 0)
            {
                highestFrequency /= 10;
                frequencyDigits++;
            }

            // See the .NET documentation for String.Format for the full explanation 
            // of the format string specifiers, but this will left-justify each word,
            // leave two spaces between the end of the longest word and the beginning
            // of the longest frequency, right-justifying the frequency.
            return "{0, -" + longestWordLength + "}  {1, " + frequencyDigits + "}";
        }
    }


    public class DBConnection
    {
       private static string connstring = "Server=localhost; database=pagewordcount; UID=root; password=";
        private DBConnection()
        {
        }

        private string databaseName = string.Empty;
     
        private MySqlConnection connection = null;
        public MySqlConnection Connection
        {
            get { return connection; }
        }

        private static DBConnection _instance = null;
        public static DBConnection Instance()
        {
            if (_instance == null)
                _instance = new DBConnection();
            return _instance;
        }

        public bool IsConnect()
        {
            if (Connection == null)
            {
              connection = new MySqlConnection(connstring);
              connection.Open();
            }

            return true;
        }

        public void Close()
        {
            connection.Close();
        }
    }
}