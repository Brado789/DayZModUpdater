using System;
using System.IO;
using System.Windows.Forms;
using Steamworks;
using System.Collections.Generic;

namespace WorkshopModViewer
{
    public partial class Form1 : Form
    {
        private CallResult<SteamUGCQueryCompleted_t>? _ugcQueryCallResult;
        private System.Windows.Forms.Timer _steamCallbackTimer;
        private UGCQueryHandle_t _ugcQueryHandle;
        private List<WorkshopItem> _allMods = new();
        private string selectedImagePath = string.Empty;

        public Form1()
        {
            InitializeComponent();

            cmbVisibility.Items.Clear();
            foreach (ERemoteStoragePublishedFileVisibility visibility in Enum.GetValues(typeof(ERemoteStoragePublishedFileVisibility)))
            {
                cmbVisibility.Items.Add(new ComboBoxItem
                {
                    Display = visibility.ToString().Replace("k_ERemoteStoragePublishedFileVisibility", ""),
                    Value = visibility
                });
            }
            cmbVisibility.DisplayMember = "Display";
            cmbVisibility.ValueMember = "Value";

            if (!SteamAPI.Init())
            {
                MessageBox.Show("SteamAPI failed to initialize! Is Steam running?");
                Close();
                return;
            }

            _ugcQueryCallResult = CallResult<SteamUGCQueryCompleted_t>.Create(OnUGCQueryCompleted);

            _steamCallbackTimer = new System.Windows.Forms.Timer();
            _steamCallbackTimer.Interval = 100;
            _steamCallbackTimer.Tick += (s, e) => SteamAPI.RunCallbacks();
            _steamCallbackTimer.Start();

            Load += (s, e) => QueryUserMods();
            listMods.SelectedIndexChanged += ListMods_SelectedIndexChanged;

            FormClosing += (s, e) =>
            {
                _steamCallbackTimer.Stop();
                SteamAPI.Shutdown();
            };
        }

        private void QueryUserMods()
        {
            UpdateStatus("Querying your published mods...");

            _ugcQueryHandle = SteamUGC.CreateQueryUserUGCRequest(
                SteamUser.GetSteamID().GetAccountID(),
                EUserUGCList.k_EUserUGCList_Published,
                EUGCMatchingUGCType.k_EUGCMatchingUGCType_All,
                EUserUGCListSortOrder.k_EUserUGCListSortOrder_CreationOrderDesc,
                SteamUtils.GetAppID(),
                SteamUtils.GetAppID(),
                1);

            SteamUGC.SetReturnMetadata(_ugcQueryHandle, true);
            SteamUGC.SetReturnChildren(_ugcQueryHandle, true);
            SteamUGC.SetReturnAdditionalPreviews(_ugcQueryHandle, true);
            SteamUGC.SetReturnLongDescription(_ugcQueryHandle, true);
            SteamUGC.SetReturnTotalOnly(_ugcQueryHandle, false);

            SteamAPICall_t apiCall = SteamUGC.SendQueryUGCRequest(_ugcQueryHandle);
            _ugcQueryCallResult?.Set(apiCall);
        }

        private void OnUGCQueryCompleted(SteamUGCQueryCompleted_t callback, bool ioFailure)
        {
            if (ioFailure || callback.m_eResult != EResult.k_EResultOK)
            {
                UpdateStatus("Failed to query mods.");
                return;
            }

            UpdateStatus($"Found {callback.m_unNumResultsReturned} mods.");

            if (listMods.InvokeRequired)
                listMods.Invoke(new Action(() => PopulateModsList(callback)));
            else
                PopulateModsList(callback);
        }

        private void PopulateModsList(SteamUGCQueryCompleted_t callback)
        {
            listMods.Items.Clear();
            _allMods.Clear();

            if (callback.m_unNumResultsReturned == 0)
            {
                listMods.Items.Add("(No mods found)");
                return;
            }

            for (uint i = 0; i < callback.m_unNumResultsReturned; i++)
            {
                if (SteamUGC.GetQueryUGCResult(callback.m_handle, i, out SteamUGCDetails_t details))
                {
                    string previewURL = string.Empty;
                    SteamUGC.GetQueryUGCPreviewURL(callback.m_handle, i, out previewURL, 260);

                    var item = new WorkshopItem(details)
                    {
                        PreviewURL = previewURL
                    };

                    _allMods.Add(item);
                }
            }

            FilterModList();
        }

        private void TxtSearch_TextChanged(object sender, EventArgs e)
        {
            FilterModList();
        }

        private void FilterModList()
        {
            string query = txtSearch.Text.Trim().ToLower();
            listMods.Items.Clear();

            foreach (var mod in _allMods)
            {
                if (string.IsNullOrWhiteSpace(query) || mod.Title.ToLower().Contains(query))
                {
                    listMods.Items.Add(mod);
                }
            }

            if (listMods.Items.Count == 0)
            {
                listMods.Items.Add("(No results)");
            }
        }

        private void ListMods_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (listMods.SelectedItem is WorkshopItem item)
            {
                txtTitle.Text = item.Title;
                txtDescription.Text = item.Description;
                txtTags.Text = item.Tags;

                foreach (ComboBoxItem boxItem in cmbVisibility.Items)
                {
                    if ((ERemoteStoragePublishedFileVisibility)boxItem.Value == item.Visibility)
                    {
                        cmbVisibility.SelectedItem = boxItem;
                        break;
                    }
                }

                try
                {
                    if (!string.IsNullOrWhiteSpace(item.PreviewURL))
                        picPreview.LoadAsync(item.PreviewURL);
                    else
                        picPreview.Image = null;
                }
                catch
                {
                    picPreview.Image = null;
                }

                btnPublishUpdate.Text = "Update";
                btnDeleteMod.Visible = true; // ✅ Show Delete button
            }
            else
            {
                btnPublishUpdate.Text = "Publish";
                btnDeleteMod.Visible = false; // ✅ Hide Delete button
            }
        }

        private void BtnPublishUpdate_Click(object sender, EventArgs e)
        {
            progressStep = 0;
            progressBar.Value = 0;
            progressTimer.Start();

            string folderPath = txtFolderPath.Text;
            string title = txtTitle.Text;
            string description = txtDescription.Text;
            string tags = txtTags.Text;
            var visibilityItem = cmbVisibility.SelectedItem as ComboBoxItem;

            if (visibilityItem == null || string.IsNullOrWhiteSpace(title))
            {
                MessageBox.Show("Please provide a title and select visibility.");
                return;
            }

            bool hasValidFolder = !string.IsNullOrWhiteSpace(folderPath) && Directory.Exists(folderPath);
            string changeNotes = string.Empty;

            if (hasValidFolder)
            {
                using (var notesDialog = new ChangeNotesDialog(listMods.SelectedItem is WorkshopItem ? "Update" : "Publish"))
                {
                    if (notesDialog.ShowDialog() != DialogResult.OK)
                        return;

                    changeNotes = notesDialog.ChangeNotes;
                }
            }

            progressBar.Visible = true;

            // Validate preview image (up to 8MB allowed)
            bool isPreviewValid = false;
            if (!string.IsNullOrWhiteSpace(selectedImagePath) && File.Exists(selectedImagePath))
            {
                FileInfo imageFile = new FileInfo(selectedImagePath);
                if (imageFile.Length > 2 * 1024 * 1024) // 8MB
                {
                    MessageBox.Show("The selected preview image is larger than 2MB. Please choose a smaller image.", "Image Too Large", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                    isPreviewValid = true;
                }
            }

            if (listMods.SelectedItem is WorkshopItem selectedItem)
            {
                // Update existing mod
                var handle = SteamUGC.StartItemUpdate(SteamUtils.GetAppID(), selectedItem.Id);
                SteamUGC.SetItemTitle(handle, title);
                SteamUGC.SetItemDescription(handle, description);
                SteamUGC.SetItemVisibility(handle, (ERemoteStoragePublishedFileVisibility)visibilityItem.Value);

                if (hasValidFolder)
                {
                    SteamUGC.SetItemContent(handle, folderPath);
                }

                if (isPreviewValid)
                {
                    SteamUGC.SetItemPreview(handle, selectedImagePath);
                }

                string[] tagArray = tags.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                SteamUGC.SetItemTags(handle, tagArray);

                SteamAPICall_t apiCall = SteamUGC.SubmitItemUpdate(handle, changeNotes);
                CallResult<SubmitItemUpdateResult_t>.Create().Set(apiCall, OnSubmitResult);
            }
            else
            {
                if (!hasValidFolder)
                {
                    MessageBox.Show("Please select a valid mod folder to publish a new mod.");
                    return;
                }

                SteamAPICall_t apiCall = SteamUGC.CreateItem(SteamUtils.GetAppID(), EWorkshopFileType.k_EWorkshopFileTypeCommunity);
                CallResult<CreateItemResult_t>.Create().Set(apiCall, (result, failure) =>
                {
                    if (failure || result.m_eResult != EResult.k_EResultOK)
                    {
                        MessageBox.Show("Failed to create new workshop item.");
                        return;
                    }

                    var handle = SteamUGC.StartItemUpdate(SteamUtils.GetAppID(), result.m_nPublishedFileId);
                    SteamUGC.SetItemTitle(handle, title);
                    SteamUGC.SetItemDescription(handle, description);
                    SteamUGC.SetItemContent(handle, folderPath);
                    SteamUGC.SetItemVisibility(handle, (ERemoteStoragePublishedFileVisibility)visibilityItem.Value);

                    if (isPreviewValid)
                    {
                        SteamUGC.SetItemPreview(handle, selectedImagePath);
                    }

                    string[] tagArray = tags.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    SteamUGC.SetItemTags(handle, tagArray);

                    SteamAPICall_t submitCall = SteamUGC.SubmitItemUpdate(handle, changeNotes);
                    CallResult<SubmitItemUpdateResult_t>.Create().Set(submitCall, OnSubmitResult);
                });
            }
        }

        private void ProgressTimer_Tick(object sender, EventArgs e)
        {
            if (progressStep < 100)
            {
                progressStep += 2;
                progressBar.Value = Math.Min(progressStep, 100);
            }
            else
            {
                progressTimer.Stop();
            }
        }
        private void OnSubmitResult(SubmitItemUpdateResult_t result, bool ioFailure)
        {
            progressTimer.Stop();
            progressBar.Value = 100;
            progressBar.Visible = false;

            if (ioFailure || result.m_eResult != EResult.k_EResultOK)
            {
                string message = result.m_eResult switch
                {
                    EResult.k_EResultInsufficientPrivilege => "You do not have permission to update this item.",
                    EResult.k_EResultTimeout => "The upload timed out. Please check your connection and try again.",
                    EResult.k_EResultNotLoggedOn => "Steam is not running or you're not logged in.",
                    EResult.k_EResultFileNotFound => "The specified mod content could not be found.",
                    EResult.k_EResultLimitExceeded => "Upload failed: The file size or update frequency limit has been exceeded.",
                    EResult.k_EResultFail => "General failure. Steam could not complete the request.",
                    _ => $"Upload failed with Steam error: {result.m_eResult}"
                };

                MessageBox.Show(message, "Upload Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string url = $"https://steamcommunity.com/sharedfiles/filedetails/?id={result.m_nPublishedFileId}";

            var open = MessageBox.Show(
                "Upload successful!\n\nWould you like to view your mod on the Steam Workshop?",
                "Upload Complete",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Information);

            if (open == DialogResult.Yes)
            {
                try
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = url,
                        UseShellExecute = true
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Could not open browser:\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }

            // Optional UI reset
            Task.Delay(1000).ContinueWith(_ =>
            {
                if (!this.IsDisposed)
                {
                    this.Invoke(new Action(() => progressBar.Value = 0));
                }
            });
        }

        private void BtnBrowse_Click(object sender, EventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Select the mod content folder";
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    string selectedPath = dialog.SelectedPath;
                    string folderName = new DirectoryInfo(selectedPath).Name;

                    bool hasCorrectName = folderName.StartsWith("@");
                    bool hasAddons = Directory.Exists(Path.Combine(selectedPath, "Addons")) ||
                                     Directory.Exists(Path.Combine(selectedPath, "addons"));

                    if (!hasCorrectName || !hasAddons)
                    {
                        MessageBox.Show("Selected folder must start with '@' and contain an 'Addons' or 'addons' folder.", "Invalid Folder", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    txtFolderPath.Text = selectedPath;
                }
            }
        }
        private void BtnClear_Click(object sender, EventArgs e)
        {
            // Reset all input fields
            txtTitle.Clear();
            txtDescription.Clear();
            txtTags.Clear();
            txtFolderPath.Clear();
            cmbVisibility.SelectedIndex = -1;
            picPreview.Image = null;
            listMods.ClearSelected();
            selectedImagePath = string.Empty;
            lblStatus.Text = "Cleared. Ready to publish a new mod.";
        }

        private void CmbVisibility_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;

            ComboBox combo = sender as ComboBox;
            string text = combo.Items[e.Index].ToString();

            e.DrawBackground();
            using (Brush background = new SolidBrush(Color.Black))
            using (Brush foreground = new SolidBrush(Color.Lime))
            {
                e.Graphics.FillRectangle(background, e.Bounds);
                e.Graphics.DrawString(text, e.Font, foreground, e.Bounds);
            }
            e.DrawFocusRectangle();
        }
        private void BtnAddImage_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Title = "Select an image for the preview";
                dialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.gif;*.bmp";

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    selectedImagePath = dialog.FileName;
                    try
                    {
                        picPreview.Image = Image.FromFile(selectedImagePath);
                    }
                    catch
                    {
                        MessageBox.Show("Failed to load image preview.");
                        selectedImagePath = string.Empty;
                    }
                }
            }
        }
        private void BtnDeleteMod_Click(object sender, EventArgs e)
        {
            if (listMods.SelectedItem is not WorkshopItem selectedItem)
                return;

            var confirm = MessageBox.Show(
                $"Are you sure you want to delete the mod:\n\n\"{selectedItem.Title}\"?",
                "Confirm Deletion",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (confirm != DialogResult.Yes)
                return;

            SteamAPICall_t deleteCall = SteamUGC.DeleteItem(selectedItem.Id);
            CallResult<DeleteItemResult_t>.Create().Set(deleteCall, (result, ioFailure) =>
            {
                if (ioFailure || result.m_eResult != EResult.k_EResultOK)
                {
                    MessageBox.Show("Failed to delete mod: " + result.m_eResult, "Deletion Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                this.Invoke(new Action(() =>
                {
                    lblStatus.Text = $"Deleted mod: {selectedItem.Title}";
                    listMods.Items.Remove(selectedItem);
                    _allMods.Remove(selectedItem);
                    btnDeleteMod.Visible = false;
                    BtnClear_Click(null, null); // Reset fields
                }));
            });
        }

        private void UpdateStatus(string text)
        {
            if (lblStatus.InvokeRequired)
                lblStatus.Invoke(new Action(() => lblStatus.Text = text));
            else
                lblStatus.Text = text;
        }
    }

    public class WorkshopItem
    {
        public PublishedFileId_t Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Tags { get; set; }
        public string PreviewURL { get; set; } = string.Empty;
        public ERemoteStoragePublishedFileVisibility Visibility { get; set; }
        public EWorkshopFileType FileType { get; set; }

        public WorkshopItem(SteamUGCDetails_t details)
        {
            Id = details.m_nPublishedFileId;
            Title = details.m_rgchTitle;
            Description = details.m_rgchDescription;
            Tags = details.m_rgchTags;
            Visibility = details.m_eVisibility;
            FileType = details.m_eFileType;
        }

        public override string ToString()
        {
            return Title;
        }
    }

    public class ComboBoxItem
    {
        public string Display { get; set; } = "";
        public object Value { get; set; }

        public override string ToString() => Display;
    }
}
