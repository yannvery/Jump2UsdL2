/******************************************************************
 * Yann VERY <BeSensaas>
 * 
 * 
 * 
 ******************************************************************/

using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using System.Configuration;
using System.Xml;
using System.Web;

/// <summary>
/// Quelques fonctions bien pratiques
/// </summary>
public class wsTools
{
    /// <summary>
    /// Crée un nouveau noeud XML
    /// </summary>
    /// <param name="name">Nom qualifié de l'élément</param>
    /// <param name="text">Valeur du noeud</param>
    /// <param name="xmlDoc">Document XML permettant d'instancier de nouveaux noeuds</param>
    /// <param name="parent">Noeud XML auquel sera rattaché le noeud créé</param>
    /// <returns>Noeud nouvellement créé</returns>
    public XmlElement CreateXmlNode(string name, string text, XmlDocument xmlDoc, XmlNode parent)
    {
        // Noeud XML créé
        XmlElement node = null;

        // Crée un nouveau noeud XML
        node = xmlDoc.CreateElement(name);

        // Définit le contenu littéral du noeud XML
        if (text != null && text != "") { node.InnerText = text; }

        // Raccorde le noeud courant à un noeud parent
        if (parent != null) { parent.AppendChild(node); }

        // Renvoie le noeud XML nouvellement créé
        return node;
    }

    /// <summary>
    /// Exécute une requête SQL et renvoie le résultat obtenu dans un DataSet
    /// </summary>
    /// <param name="sqlQuery">Requête SQL à exécuter</param>
    /// <param name="connexionString">Chaîne de connexion à la base de données</param>
    /// <returns>DataSet contenant le résultat de la requête SQL</returns>
    public DataSet ExecuteQuery(string sqlQuery, string connectionString)
    {
        SqlConnection sgdConnexion = null;				// Connexion à la base de données SGD
        SqlDataAdapter sgdAdapter = null;				// Permet de construire un DataSet à partir du résultat de la requête
        DataSet dsResult = null;						// DataSet de retour

        // Initialisation
        dsResult = new DataSet();

        // Ouvre une connexion à la base de données SGD
        sgdConnexion = new SqlConnection(connectionString);
        sgdConnexion.Open();

        // Exécute la requête et récupère les résultats dans un DataSet
        sgdAdapter = new SqlDataAdapter(sqlQuery, sgdConnexion);
        sgdAdapter.SelectCommand.CommandTimeout = 120;
        sgdAdapter.Fill(dsResult);

        // Libère les ressources
        sgdAdapter.Dispose();
        sgdConnexion.Close();
        sgdConnexion.Dispose();

        // Renvoie le DataSet contenant le résultat de la requête SQL
        return dsResult;
    }

    /// <summary>
    /// Exécute une requête SQL et renvoie le résultat obtenu dans une chaîne de caractères
    /// </summary>
    /// <param name="query">Requête SQL à exécuter</param>
    /// <param name="connection">Lien de connexion à la base de données</param>
    /// <returns>String contenant le résultat de la requête SQL</returns>
    public string DoSelect(string query, SqlConnection connection)
    {
        SqlCommand mySqlCommand = null;
        SqlDataReader mySqlDataReader = null;

        string myReslut = string.Empty;

        mySqlCommand = new SqlCommand(query, connection);
        mySqlDataReader = mySqlCommand.ExecuteReader();

        // On récupère uniquement une entrée si le DataReader contient quelque chose
        if (mySqlDataReader.Read())
        {
            myReslut = mySqlDataReader.GetValue(0).ToString();
        }

        // Libère les ressources
        mySqlDataReader.Close();
        mySqlDataReader.Dispose();

        // Renvoie la valeur recherchée
        return myReslut;
    }

    /// <summary>
    /// Enregistre une ligne dans un fichier de log
    /// </summary>
    /// <param name="code">Le code informatif de la ligne de log</param>
    /// <param name="logline">La chaîne de charactère à insérer dans le fichier</param>
    /// <returns>rien</returns>
    public void log(string code, string logline)
    {
        string myPath;
        string myFilePath;

        int i = 0;

        HttpContext hcCurrent = HttpContext.Current;

        DateTime dtLogdate = DateTime.Now;

        myPath = hcCurrent.Server.MapPath("App_Data/");
        myFilePath = myPath + "Jump2UsdL1.log.0";

        FileInfo logfile = new FileInfo(myFilePath);

        StreamWriter sw = null;

        // Le fichier .0 existe ?
        if (!logfile.Exists)
        {
            sw = logfile.CreateText();
        }
        // Le fichier .0 a une taille supérieur à 10Mo ?
        else if (logfile.Length >= 10240000)
        {
            // On récupère le prochain fichier valide
            while (logfile.Exists && logfile.Length >= 10240000)
            {
                i++;
                myFilePath = myPath + "Jump2UsdL1.log." + i;

                FileInfo next_logfile = new FileInfo(myFilePath);

                // Gestion de la rotation des logs avec comparaison de dernière modification
                if (next_logfile.Exists && logfile.LastWriteTime.CompareTo(next_logfile.LastWriteTime) > 0)
                {
                    // On supprime le fichier +1
                    next_logfile.Delete();
                    break;
                }
                else
                {
                    logfile = new FileInfo(myFilePath);
                }
            }

            // Rotation sur 10 fichiers
            if (i == 10)
            {
                myFilePath = myPath + "Jump2UsdL1.log.0";
                logfile = new FileInfo(myFilePath);
                sw = logfile.CreateText();
            }
            else
            {
                sw = new StreamWriter(myFilePath, true, Encoding.Default);
            }
        }
        else
        {
            sw = new StreamWriter(myFilePath, true, Encoding.Default);
        }

        // Ecriture dans le fichier de la date, code erreur et l'information à loguer
        sw.WriteLine(dtLogdate + " " + code + " " + logline);
        sw.Close();
    }

    public static DateTime ConvertFromUnixTimestamp(double timestamp)
    {
        DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
        return origin.AddSeconds(timestamp);
    }


    public static double ConvertToUnixTimestamp(DateTime date)
    {
        DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
        TimeSpan diff = date - origin;
        return Math.Floor(diff.TotalSeconds);
    }
}
