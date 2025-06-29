using Steamworks;
using System.Drawing;
using System.IO;

namespace WorkshopModViewer
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.ListBox listMods;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.TextBox txtTitle;
        private System.Windows.Forms.TextBox txtDescription;
        private System.Windows.Forms.TextBox txtTags;
        private System.Windows.Forms.ComboBox cmbVisibility;
        private System.Windows.Forms.PictureBox picPreview;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblDescription;
        private System.Windows.Forms.Label lblTags;
        private System.Windows.Forms.Label lblVisibility;
        private System.Windows.Forms.Label lblFolderPath;
        private System.Windows.Forms.TextBox txtFolderPath;
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.Label lblSearch;
        private System.Windows.Forms.TextBox txtSearch;
        private System.Windows.Forms.Button btnPublishUpdate;
        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.Timer progressTimer;
        private int progressStep = 0;
        private System.Windows.Forms.Button btnAddImage;
        private System.Windows.Forms.Button btnDeleteMod;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.listMods = new System.Windows.Forms.ListBox();
            this.lblStatus = new System.Windows.Forms.Label();
            this.txtTitle = new System.Windows.Forms.TextBox();
            this.txtDescription = new System.Windows.Forms.TextBox();
            this.txtTags = new System.Windows.Forms.TextBox();
            this.cmbVisibility = new System.Windows.Forms.ComboBox();
            this.picPreview = new System.Windows.Forms.PictureBox();
            this.lblTitle = new System.Windows.Forms.Label();
            this.lblDescription = new System.Windows.Forms.Label();
            this.lblTags = new System.Windows.Forms.Label();
            this.lblVisibility = new System.Windows.Forms.Label();
            this.lblFolderPath = new System.Windows.Forms.Label();
            this.txtFolderPath = new System.Windows.Forms.TextBox();
            this.btnBrowse = new System.Windows.Forms.Button();
            this.lblSearch = new System.Windows.Forms.Label();
            this.txtSearch = new System.Windows.Forms.TextBox();
            this.btnPublishUpdate = new System.Windows.Forms.Button();

            ((System.ComponentModel.ISupportInitialize)(this.picPreview)).BeginInit();
            this.SuspendLayout();

            int marginLeft = 23;
            int marginTop = 23;
            Color bgColor = Color.Black;
            Color fgColor = Color.Lime;
            Font largerFont = new Font("Segoe UI", 11F, FontStyle.Regular);
            Font labelFont = new Font("Segoe UI", 12.5F, FontStyle.Bold);

            this.listMods.FormattingEnabled = true;
            this.listMods.Location = new Point(marginLeft, marginTop + 35);
            this.listMods.Size = new Size(345, 410);
            this.listMods.Font = largerFont;
            this.listMods.BackColor = bgColor;
            this.listMods.ForeColor = fgColor;

            this.lblSearch.Text = "Search:";
            this.lblSearch.Size = new Size(70, 28);
            this.lblSearch.Location = new Point(this.listMods.Left, this.listMods.Top - 35);
            this.lblSearch.ForeColor = fgColor;
            this.lblSearch.BackColor = Color.Transparent;
            this.lblSearch.Font = labelFont;

            this.txtSearch.Location = new Point(this.lblSearch.Right + 5, this.lblSearch.Top);
            this.txtSearch.Size = new Size(this.listMods.Width - this.lblSearch.Width - 5, 28);
            this.txtSearch.Font = largerFont;
            this.txtSearch.TextChanged += new EventHandler(this.TxtSearch_TextChanged);
            this.txtSearch.BackColor = bgColor;
            this.txtSearch.ForeColor = fgColor;

            Label[] labels = new Label[] {
                lblSearch, lblTitle, lblDescription, lblTags, lblVisibility, lblFolderPath
            };
            foreach (var label in labels)
            {
                label.ForeColor = fgColor;
                label.BackColor = Color.Transparent;
                label.Font = labelFont;
            }

            this.picPreview.Location = new Point(736, marginTop);
            this.picPreview.Size = new Size(253, 253);
            this.picPreview.SizeMode = PictureBoxSizeMode.Zoom;
            this.picPreview.BackColor = Color.Black;

            this.lblTitle.Location = new Point(390, marginTop);
            this.lblTitle.Text = "Title";
            this.txtTitle.Location = new Point(390, marginTop + 25);
            this.txtTitle.Size = new Size(322, 28);
            this.txtTitle.Font = largerFont;
            this.txtTitle.BackColor = bgColor;
            this.txtTitle.ForeColor = fgColor;

            this.lblDescription.Location = new Point(390, marginTop + 60);
            this.lblDescription.Size = new Size(120, 28);
            this.lblDescription.Text = "Description";
            this.lblDescription.Font = labelFont;
            this.lblDescription.ForeColor = fgColor;

            this.txtDescription.Location = new Point(390, marginTop + 85);
            this.txtDescription.Size = new Size(330, 130);
            this.txtDescription.Font = largerFont;
            this.txtDescription.Multiline = true;
            this.txtDescription.ScrollBars = ScrollBars.Vertical;
            this.txtDescription.BackColor = bgColor;
            this.txtDescription.ForeColor = fgColor;

            this.lblTags.Location = new Point(390, marginTop + 225);
            this.lblTags.Text = "Tags";
            this.txtTags.Location = new Point(390, marginTop + 250);
            this.txtTags.Size = new Size(322, 28);
            this.txtTags.Font = largerFont;
            this.txtTags.BackColor = bgColor;
            this.txtTags.ForeColor = fgColor;

            this.lblVisibility.Location = new Point(390, marginTop + 285);
            this.lblVisibility.Text = "Visibility";
            this.cmbVisibility.Location = new Point(390, marginTop + 310);
            this.cmbVisibility.Size = new Size(300, 30);
            this.cmbVisibility.Font = largerFont;
            this.cmbVisibility.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cmbVisibility.BackColor = bgColor;
            this.cmbVisibility.ForeColor = fgColor;
            this.cmbVisibility.DrawMode = DrawMode.OwnerDrawFixed;
            this.cmbVisibility.DrawItem += new DrawItemEventHandler(CmbVisibility_DrawItem);

            this.lblFolderPath.Location = new Point(390, marginTop + 345);
            this.lblFolderPath.Size = new Size(150, 28);
            this.lblFolderPath.Text = "Mod Folder Path";
            this.txtFolderPath.Location = new Point(390, marginTop + 375);
            this.txtFolderPath.Size = new Size(264, 28);
            this.txtFolderPath.Font = largerFont;
            this.txtFolderPath.BackColor = bgColor;
            this.txtFolderPath.ForeColor = fgColor;
            this.btnBrowse.Location = new Point(660, marginTop + 370);
            this.btnBrowse.Size = new Size(52, 30);
            this.btnBrowse.Text = "...";
            this.btnBrowse.Click += new EventHandler(this.BtnBrowse_Click);

            this.btnPublishUpdate.Location = new Point(390, marginTop + 415);
            this.btnPublishUpdate.Size = new Size(115, 35);
            this.btnPublishUpdate.Text = "Publish";
            this.btnPublishUpdate.Font = largerFont;
            this.btnPublishUpdate.Click += new EventHandler(this.BtnPublishUpdate_Click);
            this.btnPublishUpdate.BackColor = bgColor;
            this.btnPublishUpdate.ForeColor = fgColor;

            this.btnAddImage = new System.Windows.Forms.Button();
            this.btnAddImage.Location = new Point(picPreview.Left, picPreview.Bottom + 40);
            this.btnAddImage.Size = new Size(picPreview.Width, 30);
            this.btnAddImage.Text = "Update Image";
            this.btnAddImage.Font = largerFont;
            this.btnAddImage.BackColor = bgColor;
            this.btnAddImage.ForeColor = fgColor;
            this.btnAddImage.Click += new EventHandler(this.BtnAddImage_Click);

            this.lblStatus.Location = new Point(marginLeft, 480);
            this.lblStatus.Size = new Size(350, 23);
            this.lblStatus.TextAlign = ContentAlignment.MiddleLeft;
            this.lblStatus.Font = new Font("Segoe UI", 10F);
            this.lblStatus.ForeColor = fgColor;
            this.lblStatus.BackColor = bgColor;

            this.btnClear = new System.Windows.Forms.Button();
            this.btnClear.Location = new Point(510, marginTop + 415);
            this.btnClear.Size = new Size(115, 35);
            this.btnClear.Text = "Clear";
            this.btnClear.Font = largerFont;
            this.btnClear.Click += new EventHandler(this.BtnClear_Click);
            this.btnClear.BackColor = bgColor;
            this.btnClear.ForeColor = fgColor;

            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.progressBar.Location = new Point(picPreview.Left, picPreview.Bottom + 10);
            this.progressBar.Size = new Size(picPreview.Width, 20);
            this.progressBar.Style = ProgressBarStyle.Marquee;
            this.progressBar.MarqueeAnimationSpeed = 30;
            this.progressBar.Visible = false;
            this.progressBar.Anchor = AnchorStyles.Top | AnchorStyles.Right;

            this.progressTimer = new System.Windows.Forms.Timer(this.components);
            this.progressTimer.Interval = 200;
            this.progressTimer.Tick += new EventHandler(this.ProgressTimer_Tick);

            this.btnDeleteMod = new System.Windows.Forms.Button();
            this.btnDeleteMod.Location = new Point(630, marginTop + 415);
            this.btnDeleteMod.Size = new Size(115, 35);
            this.btnDeleteMod.Text = "Delete";
            this.btnDeleteMod.Font = largerFont;
            this.btnDeleteMod.BackColor = bgColor;
            this.btnDeleteMod.ForeColor = fgColor;
            this.btnDeleteMod.Visible = false;
            this.btnDeleteMod.Click += new EventHandler(this.BtnDeleteMod_Click);

            this.ClientSize = new Size(1035, 529);
            this.Controls.Add(this.lblSearch);
            this.Controls.Add(this.txtSearch);
            this.Controls.Add(this.listMods);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.txtTitle);
            this.Controls.Add(this.txtDescription);
            this.Controls.Add(this.txtTags);
            this.Controls.Add(this.cmbVisibility);
            this.Controls.Add(this.picPreview);
            this.Controls.Add(this.lblTitle);
            this.Controls.Add(this.lblDescription);
            this.Controls.Add(this.lblTags);
            this.Controls.Add(this.lblVisibility);
            this.Controls.Add(this.lblFolderPath);
            this.Controls.Add(this.txtFolderPath);
            this.Controls.Add(this.btnBrowse);
            this.Controls.Add(this.btnPublishUpdate);
            this.Controls.Add(this.btnClear);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.btnAddImage);
            this.Controls.Add(this.btnDeleteMod);

            using (var ms = new System.IO.MemoryStream(SteamWorkshopUploader.Properties.Resources.exebackground))
            {
                this.BackgroundImage = Image.FromStream(ms);
            }
            this.BackgroundImageLayout = ImageLayout.Stretch;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Text = "DayZ Mod Updater";

            ((System.ComponentModel.ISupportInitialize)(this.picPreview)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}