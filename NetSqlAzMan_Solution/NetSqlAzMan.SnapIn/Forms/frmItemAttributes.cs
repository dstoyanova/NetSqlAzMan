using System;
using System.Collections.Generic;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using NetSqlAzMan.Interfaces;
using NetSqlAzMan.SnapIn.DirectoryServices.ADObjectPicker;
using NetSqlAzMan.SnapIn.DirectoryServices;
using NetSqlAzMan;

namespace NetSqlAzMan.SnapIn.Forms
{
    public partial class frmItemAttributes : frmBase
    {
        internal IAzManItem item;
        private List<KeyValuePair<string, string>> attributesToAdd;
        private List<KeyValuePair<string, string>> attributesToRemove;
        private List<KeyValuePair<string, string>> attributesToUpdate;
        private bool modified;
        private KeyValuePair<string, string> keyValueNull = new KeyValuePair<string, string>(null, null);

        public frmItemAttributes()
        {
            InitializeComponent();
            this.attributesToAdd = new List<KeyValuePair<string, string>>();
            this.attributesToRemove = new List<KeyValuePair<string, string>>();
            this.attributesToUpdate = new List<KeyValuePair<string, string>>();
        }

        private void PreSortListView(ListView lv)
        {
            lv.Sorting = SortOrder.Ascending;
        }
        private void SortListView(ListView lv)
        {
            if (lv.Sorting == SortOrder.Ascending)
                lv.Sorting = SortOrder.Descending;
            else
                lv.Sorting = SortOrder.Ascending;
            lv.Sort();
        }

        private void frmItemAttributes_Load(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.None;
            this.refreshItemAttributes();
            this.modified = false;
            this.btnApply.Enabled = false;
            this.PreSortListView(this.lsvAttributes);
            //PorkAround: http://lab.msdn.microsoft.com/ProductFeedback/viewFeedback.aspx?feedbackId=FDBK49664
            ImageList clonedImageList = new ImageList();
            foreach (Image image in this.imageList1.Images)
            {
                clonedImageList.Images.Add((Image)image.Clone());
            }
            this.lsvAttributes.SmallImageList = clonedImageList;
            //PorkAround End
            NetSqlAzMan.SnapIn.Globalization.ResourcesManager.CollectResources(this);
        }

        private ListViewItem CreateItemAttributeListViewItem(IAzManAttribute<IAzManItem> Attribute)
        {
            ListViewItem lvi = new ListViewItem();
            lvi.Tag = Attribute;
            lvi.ImageIndex = 0;
            lvi.Text = Attribute.Key;
            lvi.SubItems.Add(Attribute.Value);
            return lvi;
        }

        private ListViewItem CreateItemAttributeListViewItem(KeyValuePair<string, string> keyValue)
        {
            ListViewItem lvi = new ListViewItem();
            lvi.Tag = keyValue;
            lvi.ImageIndex = 0;
            lvi.Text = keyValue.Key;
            lvi.SubItems.Add(keyValue.Value);
            return lvi;
        }

        private void RemoveListViewItem(string key)
        {
            foreach (ListViewItem lvi in this.lsvAttributes.Items)
            {
                if (lvi.Text == key)
                {
                    lvi.Remove();
                    return;
                }
            }
        }
        private void refreshItemAttributes()
        {
            this.HourGlass(true);
            this.lsvAttributes.Items.Clear();
            IAzManAttribute<IAzManItem>[] itemAttributes = this.item.GetAttributes();
            foreach (IAzManAttribute<IAzManItem> itemAttribute in itemAttributes)
            {

                KeyValuePair<string, string> attrToUpdate = ((KeyValuePair<string, string>)this.FindAttribute(this.attributesToUpdate, itemAttribute.Key));
                if (attrToUpdate.Key == null)
                {
                    this.lsvAttributes.Items.Add(this.CreateItemAttributeListViewItem(itemAttribute));
                }
                else
                {
                    this.lsvAttributes.Items.Add(this.CreateItemAttributeListViewItem(attrToUpdate));
                }
            }
            //Add uncommitted sids 
            foreach (KeyValuePair<string, string> keyValue in this.attributesToAdd)
            {
                this.lsvAttributes.Items.Add(this.CreateItemAttributeListViewItem(keyValue));
            }
            //Remove uncommitted sids
            foreach (KeyValuePair<string, string> keyValue in this.attributesToRemove)
            {
                this.RemoveListViewItem(keyValue.Key);
            }
            this.btnApply.Enabled = this.modified;
            if (!this.item.Application.IAmManager)
                this.btnAdd.Enabled = this.btnEdit.Enabled = this.btnRemove.Enabled = this.btnOK.Enabled = this.btnApply.Enabled = false;

            this.HourGlass(false);
        }

        protected void HourGlass(bool switchOn)
        {
            this.Cursor = switchOn ? Cursors.WaitCursor : Cursors.Arrow;
            /*Application.DoEvents();*/
        }

        protected void ShowError(string text, string caption)
        {
            MessageBox.Show(text, caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        protected void ShowInfo(string text, string caption)
        {
            MessageBox.Show(text, caption, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        protected void ShowWarning(string text, string caption)
        {
            MessageBox.Show(text, caption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void frmItemAttributes_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (this.DialogResult == DialogResult.None)
                this.DialogResult = DialogResult.Cancel;
        }

        private IAzManAttribute<IAzManItem> FindAttribute(IAzManAttribute<IAzManItem>[] attributes, string key)
        {
            foreach (IAzManAttribute<IAzManItem> a in attributes)
            {
                if (a.Key == key)
                    return a;
            }
            return null;
        }

        private KeyValuePair<string, string> FindAttribute(List<KeyValuePair<string, string>> keyValues, string key)
        {
            foreach (KeyValuePair<string, string> keyValue in keyValues)
            {
                if (keyValue.Key == key)
                {
                    return keyValue;
                }
            }
            return this.keyValueNull;
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            frmAttributeProperties frm = new frmAttributeProperties();
            DialogResult dr = frm.ShowDialog(this);
            if (dr == DialogResult.OK)
            {
                KeyValuePair<string, string> keyValueToRemove = this.FindAttribute(this.attributesToRemove, frm.key);
                KeyValuePair<string, string> keyValue1 = this.FindAttribute(this.attributesToAdd, frm.key);
                IAzManAttribute<IAzManItem> itemAttribute = this.FindAttribute(this.item.GetAttributes(), frm.key);
                if (keyValueToRemove.Key == null && keyValue1.Key == null && itemAttribute == null)
                {
                    KeyValuePair<string, string> keyValueTaAdd = new KeyValuePair<string, string>(frm.key, frm.value);
                    this.attributesToAdd.Add(keyValueTaAdd);
                    ListViewItem lvi = this.CreateItemAttributeListViewItem(keyValueTaAdd);
                    this.lsvAttributes.Items.Add(lvi);
                    lvi.Selected = true;
                    this.modified = true;
                }
                else
                {
                    if (keyValueToRemove.Key == null)
                    {
                        this.ShowError(Globalization.MultilanguageResource.GetString("frmApplicationAttributes_Msg10"), Globalization.MultilanguageResource.GetString("frmApplicationAttributes_Tit10"));
                        return;
                    }
                    else
                    {
                        this.ShowError(Globalization.MultilanguageResource.GetString("frmApplicationAttributes_Msg20"), Globalization.MultilanguageResource.GetString("frmApplicationAttributes_Tit20"));
                        return;
                    }
                }
                this.modified = true;
                this.btnApply.Enabled = true;
                this.lsvAttributes.Focus();
            }
            this.HourGlass(false);
        }

        private void btnMembersRemove_Click(object sender, EventArgs e)
        {
            this.HourGlass(true);
            foreach (ListViewItem lvi in this.lsvAttributes.CheckedItems)
            {
                if ((lvi.Tag as IAzManAttribute<IAzManItem>) != null)
                {
                    IAzManAttribute<IAzManItem> lviTag = (IAzManAttribute<IAzManItem>)(lvi.Tag);
                    this.attributesToRemove.Add(new KeyValuePair<string, string>(lviTag.Key, lviTag.Value));
                    this.modified = true;
                }
                else // if ((lvi.Tag as KeyValuePair<string, string>) != null)
                {
                    KeyValuePair<string, string> lviTag = (KeyValuePair<string, string>)(lvi.Tag);
                    this.attributesToRemove.Add(lviTag);
                    this.modified = true;
                }
            }
            this.refreshItemAttributes();
            if (this.lsvAttributes.Items.Count == 0)
                this.btnRemove.Enabled = false;
            this.HourGlass(false);
        }

        private void btnApply_Click(object sender, EventArgs e)
        {
            try
            {
                this.HourGlass(true);
                this.CommitChanges();
                this.btnApply.Enabled = false;
                this.HourGlass(false);
            }
            catch (Exception ex)
            {
                this.HourGlass(false);
                this.ShowError(ex.Message, Globalization.MultilanguageResource.GetString("UpdateError_Msg10"));
            }
        }

        private void CommitChanges()
        {
            try
            {
                if (!this.modified)
                    return;
                this.item.Application.Store.Storage.BeginTransaction(AzManIsolationLevel.ReadUncommitted);
                //Attribute To Remove
                foreach (KeyValuePair<string, string> Attribute in this.attributesToRemove)
                {
                    this.item.GetAttribute(Attribute.Key).Delete();
                }
                //Attribute To Add
                foreach (KeyValuePair<string, string> Attribute in this.attributesToAdd)
                {
                    this.item.CreateAttribute(Attribute.Key, Attribute.Value);
                }
                //Attribute To Update
                foreach (KeyValuePair<string, string> Attribute in this.attributesToUpdate)
                {
                    this.item.GetAttribute(Attribute.Key).Update(Attribute.Key, Attribute.Value);
                }
                this.attributesToAdd.Clear();
                this.attributesToRemove.Clear();
                this.attributesToUpdate.Clear();
                this.modified = false;
                this.item.Application.Store.Storage.CommitTransaction();
            }
            catch
            {
                this.item.Application.Store.Storage.RollBackTransaction();
                throw;
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            try
            {
                this.HourGlass(true);
                this.CommitChanges();
                this.btnApply.Enabled = false;
                this.DialogResult = DialogResult.OK;
                this.HourGlass(false);
            }
            catch (Exception ex)
            {
                this.HourGlass(false);
                this.DialogResult = DialogResult.None;
                this.ShowError(ex.Message, Globalization.MultilanguageResource.GetString("UpdateError_Msg10"));
            }
        }


        private void btnRemove_Click(object sender, EventArgs e)
        {
            if (this.lsvAttributes.SelectedItems.Count > 0)
            {
                foreach (ListViewItem lvi in this.lsvAttributes.SelectedItems)
                {
                    if ((lvi.Tag as IAzManAttribute<IAzManItem>) != null)
                    {
                        IAzManAttribute<IAzManItem> lviTag = (IAzManAttribute<IAzManItem>)lvi.Tag;
                        this.attributesToRemove.Add(new KeyValuePair<string, string>(lviTag.Key, lviTag.Value));
                    }
                    else //if ((lvi.Tag as KeyValuePair<string,string>) != null)
                    {
                        KeyValuePair<string, string> lviTag = (KeyValuePair<string, string>)lvi.Tag;
                        KeyValuePair<string, string> attributeFound = this.FindAttribute(this.attributesToAdd, lviTag.Key);
                        if (attributeFound.Key != null)
                        {
                            this.attributesToAdd.Remove(attributeFound);
                        }
                        attributeFound = this.FindAttribute(this.attributesToUpdate, lviTag.Key);
                        if (attributeFound.Key != null)
                        {
                            this.attributesToUpdate.Remove(attributeFound);
                            this.attributesToRemove.Add(attributeFound);
                        }

                    }
                }
                this.modified = true;
                this.btnApply.Enabled = true;
                this.refreshItemAttributes();
            }
            this.HourGlass(false);
        }

        private void lsvAttributes_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (this.lsvAttributes.CheckedItems.Count == 1 && e.CurrentValue == CheckState.Checked && e.NewValue == CheckState.Unchecked)
                this.btnRemove.Enabled = false;
            else
                this.btnRemove.Enabled = true && this.item.Application.IAmManager;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            frmAttributeProperties frm = new frmAttributeProperties();
            if (this.lsvAttributes.SelectedItems[0].Tag as IAzManAttribute<IAzManItem> != null)
            {
                IAzManAttribute<IAzManItem> attr = this.lsvAttributes.SelectedItems[0].Tag as IAzManAttribute<IAzManItem>;
                frm.key = attr.Key;
                frm.value = attr.Value;
            }
            else //if (this.lsvAttributes.SelectedItems[0].Tag as KeyValuePair<string, string>)
            {
                KeyValuePair<string, string> attr = (KeyValuePair<string, string>)this.lsvAttributes.SelectedItems[0].Tag;
                frm.key = attr.Key;
                frm.value = attr.Value;
            }
            DialogResult dr = frm.ShowDialog(this);
            if (dr == DialogResult.OK)
            {
                if (this.lsvAttributes.SelectedItems[0].Tag as IAzManAttribute<IAzManItem> != null)
                {
                    IAzManAttribute<IAzManItem> attr = this.lsvAttributes.SelectedItems[0].Tag as IAzManAttribute<IAzManItem>;
                    KeyValuePair<string, string> keyValueToEdit = new KeyValuePair<string, string>(attr.Key, frm.value);
                    this.attributesToUpdate.Add(keyValueToEdit);
                    this.lsvAttributes.SelectedItems[0].SubItems[1].Text = keyValueToEdit.Value;
                    this.lsvAttributes.SelectedItems[0].Tag = keyValueToEdit;
                    this.lsvAttributes.Focus();
                }
                else //if (this.lsvAttributes.SelectedItems[0].Tag as KeyValuePair<string, string>)
                {
                    KeyValuePair<string, string> attr = this.FindAttribute(this.attributesToUpdate, frm.key);
                    if (attr.Key == null)
                        attr = this.FindAttribute(this.attributesToAdd, frm.key);
                    KeyValuePair<string, string> newAttr = new KeyValuePair<string, string>(attr.Key, frm.value);
                    this.attributesToUpdate.Remove(attr);
                    this.attributesToUpdate.Add(newAttr);
                    this.lsvAttributes.SelectedItems[0].SubItems[1].Text = newAttr.Value;
                    this.lsvAttributes.SelectedItems[0].Tag = newAttr;
                    this.lsvAttributes.Focus();
                }
                this.modified = true;
                this.btnApply.Enabled = true;
            }
            this.HourGlass(false);
        }

        private void lsvAttributes_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.btnEdit.Enabled = (this.lsvAttributes.SelectedItems.Count == 1) && this.item.Application.IAmManager;
        }

        private void lsvAttributes_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            this.btnRemove.Enabled = this.lsvAttributes.SelectedItems.Count > 0 && this.item.Application.IAmManager;
            this.btnEdit.Enabled = this.lsvAttributes.SelectedItems.Count == 1 && this.item.Application.IAmManager;
        }

        private void lsvAttributes_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            this.SortListView(this.lsvAttributes);
        }
    }
}