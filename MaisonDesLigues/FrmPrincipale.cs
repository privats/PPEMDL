using System;
using System.Data;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Configuration;
using System.Collections.ObjectModel;
using ComposantNuite;
using BaseDeDonnees;
namespace MaisonDesLigues
{
    public partial class FrmPrincipale : Form
    {
        private string typeParticipant = "";
        /// <summary>
        /// constructeur du formulaire
        /// </summary>
        public FrmPrincipale()
        {
            InitializeComponent();
        }
        private Bdd UneConnexion;
        private String TitreApplication;
        private String IdStatutSelectionne = "";
        /// <summary>
        /// création et ouverture d'une connexion vers la base de données sur le chargement du formulaire
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FrmPrincipale_Load(object sender, EventArgs e)
        {
            UneConnexion = ((FrmLogin)Owner).UneConnexion;
            TitreApplication = ((FrmLogin)Owner).TitreApplication;
            this.Text = TitreApplication;
            comboBoxModifAtelier.Text = "Choisir";
            comboBoxModifVacation.Text = "Choisir";
            Utilitaire.RemplirComboBox(UneConnexion, comboBoxModifAtelier, "VATELIER01");
            Utilitaire.RemplirComboBox(UneConnexion, comboBoxModifVacation, "VVACATION01");
            
        }
        /// <summary>
        /// gestion de l'événement click du bouton quitter.
        /// Demande de confirmation avant de quitetr l'application.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CmdQuitter_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Voulez-vous quitter l'application ?", ConfigurationManager.AppSettings["TitreApplication"], MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
            {
                UneConnexion.FermerConnexion();
                Application.Exit();
            }
        }

        private void RadTypeParticipant_Changed(object sender, EventArgs e)
        {
            switch (((RadioButton)sender).Name)
            {
                case "RadBenevole":
                    this.GererInscriptionBenevole();
                    typeParticipant = "Benevole";
                    break;
                case "RadLicencie":
                    this.GererInscriptionLicencie();
                    typeParticipant = "Licencie";
                    break;
                case "RadIntervenant":
                    this.GererInscriptionIntervenant();
                    typeParticipant = "Intervenant";
                    break;
                default:
                    throw new Exception("Erreur interne à l'application");
            }
        }

        /// <summary>     
        /// procédure permettant d'afficher l'interface de saisie du complément d'inscription d'un intervenant.
        /// </summary>
        private void GererInscriptionIntervenant()
        {

            GrpBenevole.Visible = false;
            GrpIntervenant.Visible = true;
            GrpLicencie.Visible = false;
            PanFonctionIntervenant.Visible = true;
            GrpIntervenant.Left = 30;
            GrpIntervenant.Top = 310;
            Utilitaire.CreerDesControles(this, UneConnexion, "VSTATUT01", "Rad_", PanFonctionIntervenant, "RadioButton", this.rdbStatutIntervenant_StateChanged);
            Utilitaire.RemplirComboBox(UneConnexion, CmbAtelierIntervenant, "VATELIER01");

            CmbAtelierIntervenant.Text = "Choisir";

        }

        private void GererInscriptionLicencie()
        {
            GrpBenevole.Visible = false;
            GrpLicencie.Visible = true;
            GrpIntervenant.Visible = false;
            GrpLicencie.Left = 30;
            GrpLicencie.Top = 310;
            Utilitaire.RemplirComboBox(UneConnexion, cmbQualité, "VQUALITE01");
            cmbQualité.Text = "Choisir";
            Utilitaire.RemplirListBox(UneConnexion, ListBoxAtelier, "VATELIER01");
        }

        /// <summary>
        /// procédure permettant d'afficher l'interface de saisie des disponibilités des bénévoles.
        /// </summary>
        private void GererInscriptionBenevole()
        {

            GrpBenevole.Visible = true;
            GrpLicencie.Visible = false;
            GrpBenevole.Left = 30;
            GrpBenevole.Top = 310;
            GrpIntervenant.Visible = false;

            Utilitaire.CreerDesControles(this, UneConnexion, "VDATEBENEVOLAT01", "ChkDateB_", PanelDispoBenevole, "CheckBox", this.rdbStatutIntervenant_StateChanged);
            // on va tester si le controle à placer est de type CheckBox afin de lui placer un événement checked_changed
            // Ceci afin de désactiver les boutons si aucune case à cocher du container n'est cochée
            foreach (Control UnControle in PanelDispoBenevole.Controls)
            {
                if (UnControle.GetType().Name == "CheckBox")
                {
                    CheckBox UneCheckBox = (CheckBox)UnControle;
                    UneCheckBox.CheckedChanged += new System.EventHandler(this.ChkDateBenevole_CheckedChanged);
                }
            }


        }
        /// <summary>
        /// permet d'appeler la méthode VerifBtnEnregistreIntervenant qui déterminera le statu du bouton BtnEnregistrerIntervenant
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void rdbStatutIntervenant_StateChanged(object sender, EventArgs e)
        {
            // stocke dans un membre de niveau form l'identifiant du statut sélectionné (voir règle de nommage des noms des controles : prefixe_Id)
            this.IdStatutSelectionne = ((RadioButton)sender).Name.Split('_')[1];
            BtnEnregistrerIntervenant.Enabled = VerifBtnEnregistreIntervenant();
        }
        /// <summary>
        /// Permet d'intercepter le click sur le bouton d'enregistrement d'un bénévole.
        /// Cetteméthode va appeler la méthode InscrireBenevole de la Bdd, après avoir mis en forme certains paramètres à envoyer.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnEnregistreBenevole_Click(object sender, EventArgs e)
        {
            Collection<Int16> IdDatesSelectionnees = new Collection<Int16>();
            Int64? NumeroLicence;
            if (TxtLicenceBenevole.MaskCompleted)
            {
                NumeroLicence = System.Convert.ToInt64(TxtLicenceBenevole.Text);
            }
            else
            {
                NumeroLicence = null;
            }


            foreach (Control UnControle in PanelDispoBenevole.Controls)
            {
                if (UnControle.GetType().Name == "CheckBox" && ((CheckBox)UnControle).Checked)
                {
                    /* Un name de controle est toujours formé come ceci : xxx_Id où id représente l'id dans la table
                     * Donc on splite la chaine et on récupére le deuxième élément qui correspond à l'id de l'élément sélectionné.
                     * on rajoute cet id dans la collection des id des dates sélectionnées
                        
                    */
                    IdDatesSelectionnees.Add(System.Convert.ToInt16((UnControle.Name.Split('_'))[1]));
                }
            }
            UneConnexion.InscrireBenevole(TxtNom.Text, TxtPrenom.Text, TxtAdr1.Text, TxtAdr2.Text != "" ? TxtAdr2.Text : null, TxtCp.Text, TxtVille.Text, txtTel.MaskCompleted ? txtTel.Text : null, TxtMail.Text != "" ? TxtMail.Text : null, System.Convert.ToDateTime(TxtDateNaissance.Text), NumeroLicence, IdDatesSelectionnees);
            viderZoneDeSaisie(typeParticipant);
        }
        /// <summary>
        /// Cetet méthode teste les données saisies afin d'activer ou désactiver le bouton d'enregistrement d'un bénévole
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChkDateBenevole_CheckedChanged(object sender, EventArgs e)
        {
            BtnEnregistreBenevole.Enabled = (TxtLicenceBenevole.Text == "" || TxtLicenceBenevole.MaskCompleted) && TxtDateNaissance.MaskCompleted && Utilitaire.CompteChecked(PanelDispoBenevole) > 0;
        }

        /// <summary>
        /// Méthode qui permet d'afficher ou masquer le controle panel permettant la saisie des nuités d'un intervenant.
        /// S'il faut rendre visible le panel, on teste si les nuités possibles ont été chargés dans ce panel. Si non, on les charges 
        /// On charge ici autant de contrôles ResaNuit qu'il y a de nuits possibles
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RdbNuiteIntervenant_CheckedChanged(object sender, EventArgs e)
        {
            if (((RadioButton)sender).Name == "RdbNuiteIntervenantOui")
            {
                PanNuiteIntervenant.Visible = true;
                if (PanNuiteIntervenant.Controls.Count == 0) // on charge les nuites possibles possibles et on les affiche
                {
                    //DataTable LesDateNuites = UneConnexion.ObtenirDonnesOracle("VDATENUITE01");
                    //foreach(Dat
                    Dictionary<Int16, String> LesNuites = UneConnexion.ObtenirDatesNuites();
                    int i = 0;
                    foreach (KeyValuePair<Int16, String> UneNuite in LesNuites)
                    {
                        ComposantNuite.ResaNuite unResaNuit = new ResaNuite(UneConnexion.ObtenirDonnesOracle("VHOTEL01"), (UneConnexion.ObtenirDonnesOracle("VCATEGORIECHAMBRE01")), UneNuite.Value, UneNuite.Key);
                        unResaNuit.Left = 5;
                        unResaNuit.Top = 5 + (24 * i++);
                        unResaNuit.Visible = true;
                        //unResaNuit.click += new System.EventHandler(ComposantNuite_StateChanged);
                        PanNuiteIntervenant.Controls.Add(unResaNuit);
                    }

                }

            }
            else
            {
                PanNuiteIntervenant.Visible = false;
            }
            BtnEnregistrerIntervenant.Enabled = VerifBtnEnregistreIntervenant();
        }

        private void RdbNuiteLicencie_CheckedChanged(object sender, EventArgs e)
        {
            if (((RadioButton)sender).Name == "RdbNuiteLicencieOui")
            {
                PanNuiteLicencie.Visible = true;
                if (PanNuiteLicencie.Controls.Count == 0) // on charge les nuites possibles possibles et on les affiche
                {
                    //DataTable LesDateNuites = UneConnexion.ObtenirDonnesOracle("VDATENUITE01");
                    //foreach(Dat
                    Dictionary<Int16, String> LesNuites = UneConnexion.ObtenirDatesNuites();
                    int i = 0;
                    foreach (KeyValuePair<Int16, String> UneNuite in LesNuites)
                    {
                        ComposantNuite.ResaNuite unResaNuit = new ResaNuite(UneConnexion.ObtenirDonnesOracle("VHOTEL01"), (UneConnexion.ObtenirDonnesOracle("VCATEGORIECHAMBRE01")), UneNuite.Value, UneNuite.Key);
                        unResaNuit.Left = 5;
                        unResaNuit.Top = 5 + (24 * i++);
                        unResaNuit.Visible = true;
                        //unResaNuit.click += new System.EventHandler(ComposantNuite_StateChanged);
                        PanNuiteLicencie.Controls.Add(unResaNuit);
                    }

                }

            }
            else
            {
                PanNuiteLicencie.Visible = false;
            }
            BtnEnregistrerLicencie.Enabled = VerifBtnEnregistreLicencie();
        }

        /// <summary>
        /// Cette procédure va appeler la procédure .... qui aura pour but d'enregistrer les éléments 
        /// de l'inscription d'un intervenant, avec éventuellment les nuités à prendre en compte        ///
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnEnregistrerIntervenant_Click(object sender, EventArgs e)
        {
            try
            {
                if (RdbNuiteIntervenantOui.Checked)
                {
                    // inscription avec les nuitées
                    Collection<Int16> NuitsSelectionnes = new Collection<Int16>();
                    Collection<String> HotelsSelectionnes = new Collection<String>();
                    Collection<String> CategoriesSelectionnees = new Collection<string>();
                    foreach (Control UnControle in PanNuiteIntervenant.Controls)
                    {
                        if (UnControle.GetType().Name == "ResaNuite" && ((ResaNuite)UnControle).GetNuitSelectionnee())
                        {
                            // la nuité a été cochée, il faut donc envoyer l'hotel et la type de chambre à la procédure de la base qui va enregistrer le contenu hébergement 
                            //ContenuUnHebergement UnContenuUnHebergement= new ContenuUnHebergement();
                            CategoriesSelectionnees.Add(((ResaNuite)UnControle).GetTypeChambreSelectionnee());
                            HotelsSelectionnes.Add(((ResaNuite)UnControle).GetHotelSelectionne());
                            NuitsSelectionnes.Add(((ResaNuite)UnControle).IdNuite);
                         }

                    }
                    if (NuitsSelectionnes.Count == 0)
                    {
                        MessageBox.Show("Si vous avez sélectionné que l'intervenant avait des nuités\n il faut qu'au moins une nuit soit sélectionnée");
                    }
                    else
                    {
                        UneConnexion.InscrireIntervenant(TxtNom.Text, TxtPrenom.Text, TxtAdr1.Text, TxtAdr2.Text != "" ? TxtAdr2.Text : null, TxtCp.Text, TxtVille.Text, txtTel.MaskCompleted ? txtTel.Text : null, TxtMail.Text != "" ? TxtMail.Text : null, System.Convert.ToInt16(CmbAtelierIntervenant.SelectedValue), this.IdStatutSelectionne, CategoriesSelectionnees, HotelsSelectionnes, NuitsSelectionnes);
                        MessageBox.Show("Inscription intervenant effectuée");
                        viderZoneDeSaisie(typeParticipant);
                    }
                }
                else
                { // inscription sans les nuitées
                    UneConnexion.InscrireIntervenant(TxtNom.Text, TxtPrenom.Text, TxtAdr1.Text, TxtAdr2.Text != "" ? TxtAdr2.Text : null, TxtCp.Text, TxtVille.Text, txtTel.MaskCompleted ? txtTel.Text : null, TxtMail.Text != "" ? TxtMail.Text : null, System.Convert.ToInt16(CmbAtelierIntervenant.SelectedValue), this.IdStatutSelectionne);
                    MessageBox.Show("Inscription intervenant effectuée");
                }
                
                
            }
            catch (Exception Ex)
            {
                MessageBox.Show(Ex.Message);
            }
        }

        /// <summary>
        /// Méthode privée testant le contrôle combo et la variable IdStatutSelectionne qui contient une valeur
        /// Cette méthode permetra ensuite de définir l'état du bouton BtnEnregistrerIntervenant
        /// </summary>
        /// <returns></returns>
        private Boolean VerifBtnEnregistreIntervenant()
        {
            return CmbAtelierIntervenant.Text !="Choisir" && this.IdStatutSelectionne.Length > 0;
        }

        private Boolean VerifBtnEnregistreLicencie()
        {
            return cmbQualité.Text != "Choisir";
        }

        /// <summary>
        /// Méthode permettant de définir le statut activé/désactivé du bouton BtnEnregistrerIntervenant
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CmbAtelierIntervenant_TextChanged(object sender, EventArgs e)
        {
            BtnEnregistrerIntervenant.Enabled = VerifBtnEnregistreIntervenant();
        }

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        //private void ChkDateBenevole_CheckedChanged(object sender, KeyEventArgs e)
        //{

        //}

        private void radioAtelier_CheckedChanged(object sender, EventArgs e)
        {
            grpAjouterAtelier.Visible = true;
            grpAjouterAtelier.Left = 28;
            grpAjouterAtelier.Top = 154;
            grpAjouterVacation.Visible = false;
            grpAjouterTheme.Visible = false;
        }

        private void radioTheme_CheckedChanged(object sender, EventArgs e)
        {
            grpAjouterAtelier.Visible = false;
            grpAjouterVacation.Visible = false;
            grpAjouterTheme.Visible = true;
            grpAjouterTheme.Left = 28;
            grpAjouterTheme.Top = 154;
            Utilitaire.RemplirComboBox(UneConnexion, comboBoxAtelier_Theme, "VATELIER01");
            comboBoxAtelier_Theme.Text = "Choisir";
        }

        private void radioVacation_CheckedChanged(object sender, EventArgs e)
        {
            grpAjouterAtelier.Visible = false;
            grpAjouterVacation.Visible = true;
            grpAjouterVacation.Left = 28;
            grpAjouterVacation.Top = 154;
            grpAjouterTheme.Visible = false;
            Utilitaire.RemplirComboBox(UneConnexion, comboBoxAtelier_Vacation, "VATELIER01");
            comboBoxAtelier_Vacation.Text = "Choisir";
            Utilitaire.RemplirComboBox(UneConnexion, comboBoxVacDate, "VDATEBENEVOLAT01");
            comboBoxVacDate.Text = "Choisir";
        }

        private void buttonEnregistrerAtelier_Click(object sender, EventArgs e)
        {
            if ((String.IsNullOrEmpty(textBoxNbPlaceMaxi.Text)) || (String.IsNullOrEmpty(textBoxLibAtelier.Text)))
            {
                MessageBox.Show("Tous les champs doivent être rempli.");
            }
            else
            {
                try
                {
                    UneConnexion.AjouterAtelier(textBoxLibAtelier.Text, Int32.Parse(textBoxNbPlaceMaxi.Text));
                    radioAtelier.Checked = false;
                    grpAjouterAtelier.Visible = false;
                    textBoxNbPlaceMaxi.Text = "";
                    textBoxLibAtelier.Text = "";
                }
                catch
                {
                    MessageBox.Show("Erreur lors de la création de l'atelier.");
                }                
            }
        }

        private void buttonEnregistrerTheme_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(textBoxTheme.Text))
            {
                MessageBox.Show("Tous les champs doivent être rempli.");
            }
            else
            {
                try
                {
                    UneConnexion.AjouterTheme(textBoxTheme.Text, comboBoxAtelier_Theme.SelectedValue.ToString());
                    radioTheme.Checked = false;
                    grpAjouterTheme.Visible = false;
                    textBoxTheme.Text = "";
                    comboBoxAtelier_Theme.Text = "Choisir";
                }
                catch
                {
                    MessageBox.Show("Erreur lors de la création du thème.");
                }
            }
        }

        private void buttonEnregistrerVacation_Click(object sender, EventArgs e)
        {
            if ((comboBoxAtelier_Vacation.Text == "Choisir"))
            {
                MessageBox.Show("Tous les champs doivent être rempli.");
            }
            else
            {
                try
                {
                    string heureDeb = comboBoxVacDate.Text + " " + textBoxDebVacation.Text;
                    string heureFin = comboBoxVacDate.Text + " " + textBoxFinVacation.Text;
                    UneConnexion.AjouterVacation(comboBoxAtelier_Vacation.SelectedValue.ToString(), heureDeb, heureFin);
                    radioVacation.Checked = false;
                    grpAjouterVacation.Visible = false;
                    comboBoxAtelier_Vacation.Text = "Choisir";                    
                }
                catch
                {
                    MessageBox.Show("Erreur lors de la création de la vacation.");
                }
            }
            
        }
                
        private void buttonEnregistrerModif_Click(object sender, EventArgs e)
        {
            if ((comboBoxModifAtelier.Text == "Choisir") || (comboBoxModifVacation.Text == "Choisir") || (String.IsNullOrEmpty(textBoxDebVacationMod.Text)) || (String.IsNullOrEmpty(textBoxFinVacationMod.Text)))
            {
                MessageBox.Show("Tous les champs doivent être rempli.");
            }
            else
            {
                try
                {
                    UneConnexion.ModifVacation(comboBoxModifAtelier.SelectedValue.ToString(), comboBoxModifVacation.SelectedValue.ToString(), textBoxDebVacationMod.ToString(), textBoxFinVacationMod.ToString());
                    comboBoxModifAtelier.Text = "Choisir";
                    comboBoxModifVacation.Text = "Choisir";
                    textBoxDebVacationMod.Text = "";
                    textBoxFinVacationMod.Text = "";
                }
                catch
                {
                    MessageBox.Show("Erreur lors de la modification de la vacation.");
                }
            }
        }

        public void viderZoneDeSaisie(string typeParticipant)
        {
            TxtNom.Text = "";
            TxtPrenom.Text = "";
            TxtAdr1.Text = "";
            TxtAdr2.Text = "";
            TxtCp.Text = "";
            TxtVille.Text = "";
            txtTel.Text = "";
            TxtMail.Text = "";
            if (typeParticipant == "Intervenant")
            {
                CmbAtelierIntervenant.Text = "Choisir" ;
                RdbNuiteIntervenantOui.Checked = false;
                RdbNuiteIntervenantNon.Checked = true;
            }
            if (typeParticipant == "Licencier")
            {
                cmbQualité.Text = "Choisir";
                textLicenceLicencie.Text = "";
                radioAccNon.Checked = true;
                radioAccOui.Checked = false;
                RdbNuiteLicencieNon.Checked = true;
                RdbNuiteLicencieOui.Checked = false;
            }
            if (typeParticipant == "Benevole")
            {
                TxtDateNaissance.Text = "" ;
                TxtLicenceBenevole.Text = "" ;
            }
        }

        private void RdbNuiteLicencieOui_CheckedChanged(object sender, EventArgs e)
        {
            if (((RadioButton)sender).Name == "RdbNuiteLicencieOui")
            {
                PanNuiteLicencie.Visible = true;
                if (PanNuiteLicencie.Controls.Count == 0) // on charge les nuites possibles possibles et on les affiche
                {
                    //DataTable LesDateNuites = UneConnexion.ObtenirDonnesOracle("VDATENUITE01");
                    //foreach(Dat
                    Dictionary<Int16, String> LesNuites = UneConnexion.ObtenirDatesNuites();
                    int i = 0;
                    foreach (KeyValuePair<Int16, String> UneNuite in LesNuites)
                    {
                        ComposantNuite.ResaNuite unResaNuit = new ResaNuite(UneConnexion.ObtenirDonnesOracle("VHOTEL01"), (UneConnexion.ObtenirDonnesOracle("VCATEGORIECHAMBRE01")), UneNuite.Value, UneNuite.Key);
                        unResaNuit.Left = 5;
                        unResaNuit.Top = 5 + (24 * i++);
                        unResaNuit.Visible = true;
                        //unResaNuit.click += new System.EventHandler(ComposantNuite_StateChanged);
                        PanNuiteLicencie.Controls.Add(unResaNuit);
                    }

                }

            }            
        }

        private void RdbNuiteLicencieNon_CheckedChanged(object sender, EventArgs e)
        {
            PanNuiteLicencie.Visible = false;
        }

        private void TabInscription_Click(object sender, EventArgs e)
        {


        }

        private void BtnEnregistrerLicencie_Click(object sender, EventArgs e)
        {
             {
            try
            {
                if (RdbNuiteIntervenantOui.Checked)
                {
                    // inscription avec les nuitées
                    Collection<Int16> NuitsSelectionnes = new Collection<Int16>();
                    Collection<String> HotelsSelectionnes = new Collection<String>();
                    Collection<String> CategoriesSelectionnees = new Collection<string>();
                    foreach (Control UnControle in PanNuiteIntervenant.Controls)
                    {
                        if (UnControle.GetType().Name == "ResaNuite" && ((ResaNuite)UnControle).GetNuitSelectionnee())
                        {
                            // la nuité a été cochée, il faut donc envoyer l'hotel et la type de chambre à la procédure de la base qui va enregistrer le contenu hébergement 
                            //ContenuUnHebergement UnContenuUnHebergement= new ContenuUnHebergement();
                            CategoriesSelectionnees.Add(((ResaNuite)UnControle).GetTypeChambreSelectionnee());
                            HotelsSelectionnes.Add(((ResaNuite)UnControle).GetHotelSelectionne());
                            NuitsSelectionnes.Add(((ResaNuite)UnControle).IdNuite);
                         }

                    }
                    if (NuitsSelectionnes.Count == 0)
                    {
                        MessageBox.Show("Si vous avez sélectionné que l'intervenant avait des nuités\n il faut qu'au moins une nuit soit sélectionnée");
                    }
                    else
                    {
                        UneConnexion.InscrireIntervenant(TxtNom.Text, TxtPrenom.Text, TxtAdr1.Text, TxtAdr2.Text != "" ? TxtAdr2.Text : null, TxtCp.Text, TxtVille.Text, txtTel.MaskCompleted ? txtTel.Text : null, TxtMail.Text != "" ? TxtMail.Text : null, System.Convert.ToInt16(CmbAtelierIntervenant.SelectedValue), this.IdStatutSelectionne, CategoriesSelectionnees, HotelsSelectionnes, NuitsSelectionnes);
                        MessageBox.Show("Inscription intervenant effectuée");
                        viderZoneDeSaisie(typeParticipant);
                    }
                }
                else
                { // inscription sans les nuitées
                    UneConnexion.InscrireIntervenant(TxtNom.Text, TxtPrenom.Text, TxtAdr1.Text, TxtAdr2.Text != "" ? TxtAdr2.Text : null, TxtCp.Text, TxtVille.Text, txtTel.MaskCompleted ? txtTel.Text : null, TxtMail.Text != "" ? TxtMail.Text : null, System.Convert.ToInt16(CmbAtelierIntervenant.SelectedValue), this.IdStatutSelectionne);
                    MessageBox.Show("Inscription intervenant effectuée");
                }
                
                
            }
            catch (Exception Ex)
            {
                MessageBox.Show(Ex.Message);
            }
        }

        /*
        private void radioAccOui_CheckedChanged(object sender, EventArgs e)
        {
            {
                if (((RadioButton)sender).Name == "radioAccOui")
                {
                    panelRepas.Visible = true;
                    if (panelRepas.Controls.Count == 0) // on charge les repas possibles possibles et on les affiche
                    {                        
                        Dictionary<Int16, String> LesRepas = UneConnexion.ObtenirDatesRepas();

                        int i = 0;
                        foreach (KeyValuePair<Int16, String> UnRepas in LesRepas)
                        {
                            unResaRepas.Left = 5;
                            unResaRepas.Top = 5 + (24 * i++);
                            unResaRepas.Visible = true;
                            //unResaNuit.click += new System.EventHandler(ComposantNuite_StateChanged);
                            panelRepas.Controls.Add(unResaRepas);
                        }

                    }

                }
            }
        }*/
     }

    }

