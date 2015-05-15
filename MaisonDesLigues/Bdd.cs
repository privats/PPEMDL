using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using Oracle.DataAccess.Client;
using System.Configuration;
using System.Windows.Forms;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;  // bibliothèque pour les expressions régulières

namespace MDLParticipants
{
   public class Bdd
    {
        //
        // propriétés membres
        //
        private OracleConnection CnOracle;
        private OracleCommand UneOracleCommand;
        private OracleDataAdapter UnOracleDataAdapter;
        private DataTable UneDataTable;
        private OracleDataReader UnReader;
        private OracleTransaction UneOracleTransaction;
        private Collection<Control> LesControls;
        //
        // méthodes
        //
        /// <summary>
        /// constructeur de la connexion
        /// </summary>
        /// <param name="UnLogin">login utilisateur</param>
        /// <param name="UnPwd">mot de passe utilisateur</param>
        public Bdd(String UnLogin, String UnPwd)
        {
            ConnectionStringSettings CnString = new ConnectionStringSettings();
            try
            {
                /// <remarks>on commence par récupérer dans CnString les informations contenues dans le fichier app.config
                /// pour la connectionString de nom StrConnMdl
                /// </remarks>
                CnString = ConfigurationManager.ConnectionStrings["StrConnMdl"];
                CnOracle = new OracleConnection(string.Format(CnString.ConnectionString,
                                                    ConfigurationManager.AppSettings["SERVEROUT"],
                                                    ConfigurationManager.AppSettings["PORTOUT"],
                                                    ConfigurationManager.AppSettings["SID"],
                                                    UnLogin,
                                                    UnPwd
                                                    )
                                                );
                ///<remarks>
                /// on va remplacer dans la chaine de connexion les paramètres par le login et le pwd saisis
                ///dans les zones de texte. Pour ça on va utiliser la méthode Format de la classe String.                /// 
                /// </remarks>

                CnOracle.Open();
            }
            catch (OracleException)
            {
                // si la connexion IN a échoué, on tente la connexion out.
                try
                {
                    CnString = ConfigurationManager.ConnectionStrings["StrConnMdl"];
                    CnOracle = new OracleConnection(string.Format(CnString.ConnectionString,
                                                      ConfigurationManager.AppSettings["SERVERIN"],
                                                      ConfigurationManager.AppSettings["PORTIN"],
                                                      ConfigurationManager.AppSettings["SID"],
                                                      UnLogin,
                                                      UnPwd
                                                      )
                                                  );
                    CnOracle.Open();
                }
                catch (OracleException)
                {
                    throw new Exception("connexion impossible à la base");
                }
            }
        }
        /// <summary>
        /// Methode d'execution d'une requete
        /// </summary>
        /// <param name="UnObjetCommande"></param>
        /// <returns>DataTable</returns>
        private DataTable ExecuteRequete(OracleCommand UnObjetCommande)
        {
            UnReader = UneOracleCommand.ExecuteReader();
            UneDataTable = new DataTable();
            UneDataTable.Load(UnReader);
            UnReader.Close();
            return UneDataTable;
        }
        /// <summary>
        /// permet de récupérer le contenu d'une table ou d'une vue. 
        /// </summary>
        /// <param name="UneTableOuVue"> nom de la table ou la vue dont on veut récupérer le contenu</param>
        /// <returns>un objet de type datatable contenant les données récupérées</returns>
        public DataTable ObtenirDonnesOracle(String UneTableOuVue)
        {
            string Sql = "select * from " + UneTableOuVue;
            this.UneOracleCommand = new OracleCommand(Sql, CnOracle);
            UnOracleDataAdapter = new OracleDataAdapter();
            UnOracleDataAdapter.SelectCommand = this.UneOracleCommand;
            UneDataTable = new DataTable();
            UnOracleDataAdapter.Fill(UneDataTable);
            return UneDataTable;
        }
        /// <summary>
        /// méthode permettant de remplir une combobox à partir d'une source de données
        /// </summary>
        /// <param name="UneConnexion">L'objet connexion à utiliser pour la connexion à la BD</param>
        /// <param name="UneCombo"> La combobox que l'on doit remplir</param>
        /// <param name="UneSource">Le nom de la source de données qui va fournir les données. Il s'agit en fait d'une vue de type
        /// VXXXXOn ou XXXX représente le nom de la tabl à partir de laquelle la vue est créée. n représente un numéro de séquence</param>
        public static void RemplirComboBox(Bdd UneConnexion, ComboBox UneCombo, String UneSource)
        {

            UneCombo.DataSource = UneConnexion.ObtenirDonnesOracle(UneSource);
            UneCombo.DisplayMember = "libelle";
            UneCombo.ValueMember = "id";
        }

        /// <summary>
        /// méthode permettant de renvoyer un message d'erreur provenant de la bd
        /// après l'avoir formatté. On ne renvoie que le message, sans code erreur
        /// </summary>
        /// <param name="unMessage">message à formater</param>
        /// <returns>message formaté à afficher dans l'application</returns>
        private String GetMessageOracle(String unMessage)
        {
            String[] message = Regex.Split(unMessage, "ORA-");
            return (Regex.Split(message[1], ":"))[1];
        }

        public void AjoutDateHeureArriveeParticipant(Int16 pIdParticipant, DateTime pDateHeureArrivee,String pCleWifi)
        {
            String MessageErreur="";
            try
            {
                UneOracleCommand = new OracleCommand("mdl.pckparticipant.UPDATEARRIVEEPARTICIPANT", CnOracle);
                UneOracleCommand.CommandType = CommandType.StoredProcedure;

                UneOracleCommand.Parameters.Add("pIdParticipant", OracleDbType.Int16, ParameterDirection.Input).Value = pIdParticipant;
                UneOracleCommand.Parameters.Add("pDateHeureArrivee", OracleDbType.Date, ParameterDirection.Input).Value = pDateHeureArrivee;
                UneOracleCommand.Parameters.Add("pCleWifi", OracleDbType.Char, ParameterDirection.Input).Value = pCleWifi;
                UneOracleTransaction = this.CnOracle.BeginTransaction();
                UneOracleCommand.ExecuteNonQuery();
                UneOracleTransaction.Commit();
            }
            catch (OracleException Oex)
            {
                MessageErreur = "Erreur Oracle \n" + this.GetMessageOracle(Oex.Message);
            }
            catch (Exception ex)
            {
                MessageErreur = "Autre Erreur, " + ex.Message;
            }
            finally
            {
                if (MessageErreur.Length > 0)
                {
                    // annulation de la transaction
                    UneOracleTransaction.Rollback();
                    // Déclenchement de l'exception
                    throw new Exception(MessageErreur);
                }
            }
        }   
  }
}
