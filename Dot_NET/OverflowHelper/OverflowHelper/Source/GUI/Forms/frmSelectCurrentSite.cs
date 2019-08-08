

using System; // For EventArgs

using System.Collections.Generic; // For List

//using System.ComponentModel;
//using System.Data;
//using System.Drawing;
//using System.Linq;
using System.Windows.Forms; // For Close(), etc.

using System.Text; //For StringBuilder.

using OverflowHelper.core;


/****************************************************************************
 *    <placeholder for header>                                              *
 ****************************************************************************/
namespace OverflowHelper
{


    /****************************************************************************
     *    <placeholder for header>                                              *
     ****************************************************************************/
    public partial class frmSelectCurrentSite : Form
    {
        private Sites mSites;

        private List<string> mSitesOrder; //Whatever order sites is 
        //                                  listed in the GUI.
        

        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        public frmSelectCurrentSite(Sites aSites)
        {
            InitializeComponent();

            mSites = aSites;

            int len = aSites.getSiteURLs().Count;
            mSitesOrder = new List<string>(len);
        }


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        private void frmSelectCurrentSite_Load(object aSender, EventArgs anEvent)
        {
            // Later: let mSites know about the set of sites...

            cbSites.Items.Clear();

            List<string> siteURLS = mSites.getSiteURLs();

            foreach (string siteDomainURL in siteURLS)
            {
                siteItem someSiteItem = mSites.getSiteInfo(siteDomainURL);
                string toAdd = siteDomainURL;

                string siteName = someSiteItem.name;
                if (siteName != "")
                {
                    toAdd += " - " + siteName;
                }

                mSitesOrder.Add(siteDomainURL);
                cbSites.Items.Add(toAdd);
            }        
        }


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        private void btnOK_Click(object aSender, EventArgs anEvent)
        {
            // "askubuntu.com"
            //string newSiteDomainURL = cbSites.Text;
            string newSiteDomainURL = mSitesOrder[cbSites.SelectedIndex];

            mSites.setCurrentSiteDomainURL(newSiteDomainURL);
            this.Close();
        }


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        

        /****************************************************************************
         *    WCF tryout                                                            *
         ****************************************************************************/
        private void button1_Click(object sender, EventArgs e)
        {

            //System.Array xyz = new Array();
            //xyz.Resize(1);

            //System.Array xyz = new Array();
            //string[] abc;
            //abc.re

            string[] array1 = new string[5];
            string first = array1[0];

            ServiceReference3.Service1Client client3 = 
                new ServiceReference3.Service1Client();

            string returnString = client3.GetData("XYZ");
            label2.Text = returnString;
        }


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        private void btnExportLlistToClipboard_Click(object aSender, 
                                                     EventArgs anEvent)
        {
            //72 sites as of 2016-07-08, with 20% margin.
            StringBuilder scratchSB = new StringBuilder(2200); 

            List<string> siteURLS = mSites.getSiteURLs();
            foreach (string siteDomainURL in siteURLS)
            {
                scratchSB.Append(siteDomainURL);
                scratchSB.Append("\n");
            }

            EditorOverflowApplication.setClipboard3(scratchSB.ToString(), null);
        } //btnExportLlistToClipboard_Click()


    } //class frmSelectCurrentSite


} //namespace OverflowHelper

