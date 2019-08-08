

using System; //For 'EventArgs' 

//using System.Collections.Generic;
//using System.ComponentModel;
//using System.Data;
//using System.Drawing;
//using System.Linq;
//using System.Text;

using System.Windows.Forms;

using OverflowHelper.core;


/****************************************************************************
 *    <placeholder for header>                                              *
 ****************************************************************************/
namespace OverflowHelper // Note: if changed, it must also be changed in
//                                file SelectEditSummaryStyle.Designer.cs.
//                                And the solution must be rebuilt.
{


    /****************************************************************************
     *    <placeholder for header>                                              *
     ****************************************************************************/
    public partial class SelectEditSummaryStyle : Form
    {

        private EditSummaryStyle mEditSummaryStyle;


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        //public SelectEditSummaryStyle()
        public SelectEditSummaryStyle(EditSummaryStyle aEditSummaryStyle)
        {
            InitializeComponent();

            mEditSummaryStyle = aEditSummaryStyle;

            cbEditSummaryStyle.SelectedIndex = 
                 (int)(mEditSummaryStyle.getCurrentStyle());
        }


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        private void btnOK_Click(object aSender, EventArgs anEvent)
        {
            //// "askubuntu.com"
            //string newSiteDomainURL = mSitesOrder[cbEditSummaryStyle.SelectedIndex];
            //mSites.setCurrentSiteDomainURL(newSiteDomainURL);

            mEditSummaryStyle.setCurrentStyle(
                (EditSummaryEnum)cbEditSummaryStyle.SelectedIndex);

            this.Close();
        }


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }


    } //class SelectEditSummaryStyle


}
