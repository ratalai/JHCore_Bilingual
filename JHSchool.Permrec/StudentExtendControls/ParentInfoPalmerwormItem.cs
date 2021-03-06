using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using System.Xml;
using FISCA.DSAUtil;
using FISCA.Presentation;
using Framework;
using JHSchool.Data;
using JHSchool.Permrec.Feature.Legacy;
using FCode = Framework.Security.FeatureCodeAttribute;

namespace JHSchool.Permrec.StudentExtendControls
{
    [FCode("JHSchool.Student.Detail0083", "父母親及監護人資料")]
    internal partial class ParentInfoPalmerwormItem : FISCA.Presentation.DetailContent
    {
        private bool _isInitialized = false;
        JHParentRecord _StudParentRec;
        private ChangeListener _DataListener_Father;
        private ChangeListener _DataListener_Mother;
        private ChangeListener _DataListener_Guardian;
        private bool isBGBusy = false;
        private BackgroundWorker BGWorker;
        private BackgroundWorker _getRelationshipBackgroundWorker;
        private BackgroundWorker _getJobBackgroundWorker;
        private BackgroundWorker _getEduDegreeBackgroundWorker;
        private BackgroundWorker _getNationalityBackgroundWorker;
        private ParentType _ParentType;
        PermRecLogProcess prlp;        

        private enum ParentType
        {
            Father,
            Mother,
            Guardian
        }

        public ParentInfoPalmerwormItem()
        {
            InitializeComponent();
            Group = "父母親及監護人資料";
            prlp = new PermRecLogProcess();
            _StudParentRec = new JHParentRecord();
            _ParentType = ParentType.Guardian;

            _DataListener_Father = new ChangeListener();
            _DataListener_Guardian = new ChangeListener();
            _DataListener_Mother = new ChangeListener();
            _DataListener_Father.StatusChanged += new EventHandler<ChangeEventArgs>(_DataListener_Father_StatusChanged);
            _DataListener_Guardian.StatusChanged += new EventHandler<ChangeEventArgs>(_DataListener_Guardian_StatusChanged);
            _DataListener_Mother.StatusChanged += new EventHandler<ChangeEventArgs>(_DataListener_Mother_StatusChanged);

            // 加入父親 Listener
            _DataListener_Father.Add(new TextBoxSource(txtParentName));
            _DataListener_Father.Add(new TextBoxSource(txtIDNumber));
            _DataListener_Father.Add(new TextBoxSource(txtParentPhone));
            _DataListener_Father.Add(new TextBoxSource(txtEMail));
            _DataListener_Father.Add(new ComboBoxSource(cboNationality,ComboBoxSource.ListenAttribute.Text));
            _DataListener_Father.Add(new ComboBoxSource(cboJob,ComboBoxSource.ListenAttribute.Text));
            _DataListener_Father.Add(new ComboBoxSource(cboEduDegree, ComboBoxSource.ListenAttribute.Text));
            _DataListener_Father.Add(new ComboBoxSource(cboAlive, ComboBoxSource.ListenAttribute.Text));

            // 加入母親 Listener
            _DataListener_Mother.Add(new TextBoxSource(txtParentName));
            _DataListener_Mother.Add(new TextBoxSource(txtIDNumber));
            _DataListener_Mother.Add(new TextBoxSource(txtParentPhone));
            _DataListener_Mother.Add(new TextBoxSource(txtEMail));
            _DataListener_Mother.Add(new ComboBoxSource(cboNationality, ComboBoxSource.ListenAttribute.Text));
            _DataListener_Mother.Add(new ComboBoxSource(cboJob, ComboBoxSource.ListenAttribute.Text));
            _DataListener_Mother.Add(new ComboBoxSource(cboEduDegree, ComboBoxSource.ListenAttribute.Text));
            _DataListener_Mother.Add(new ComboBoxSource(cboAlive, ComboBoxSource.ListenAttribute.Text));            


            // 加入監護人 Listener
            _DataListener_Guardian.Add(new TextBoxSource(txtParentName));
            _DataListener_Guardian.Add(new TextBoxSource(txtIDNumber));
            _DataListener_Guardian.Add(new TextBoxSource(txtParentPhone));
            _DataListener_Guardian.Add(new TextBoxSource(txtEMail));
            _DataListener_Guardian.Add(new ComboBoxSource(cboNationality, ComboBoxSource.ListenAttribute.Text));
            _DataListener_Guardian.Add(new ComboBoxSource(cboJob, ComboBoxSource.ListenAttribute.Text));
            _DataListener_Guardian.Add(new ComboBoxSource(cboEduDegree, ComboBoxSource.ListenAttribute.Text));
            _DataListener_Guardian.Add(new ComboBoxSource(cboRelationship, ComboBoxSource.ListenAttribute.Text));

            JHParent.AfterUpdate += new EventHandler<K12.Data.DataChangedEventArgs>(JHParent_AfterUpdate);

            BGWorker = new BackgroundWorker();
            BGWorker.DoWork += new DoWorkEventHandler(BGWorker_DoWork);
            BGWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(BGWorker_RunWorkerCompleted);

            _getEduDegreeBackgroundWorker = new BackgroundWorker();            
            _getEduDegreeBackgroundWorker.DoWork += new DoWorkEventHandler(_getEduDegreeBackgroundWorker_DoWork);
            _getEduDegreeBackgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(_getEduDegreeBackgroundWorker_RunWorkerCompleted);
            _getEduDegreeBackgroundWorker.RunWorkerAsync ();

            _getNationalityBackgroundWorker = new BackgroundWorker();
            _getNationalityBackgroundWorker.DoWork += new DoWorkEventHandler(_getNationalityBackgroundWorker_DoWork);
            _getNationalityBackgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(_getNationalityBackgroundWorker_RunWorkerCompleted);
            _getNationalityBackgroundWorker.RunWorkerAsync();

            _getRelationshipBackgroundWorker = new BackgroundWorker();
            _getRelationshipBackgroundWorker.DoWork +=new DoWorkEventHandler(_getRelationshipBackgroundWorker_DoWork);
            _getRelationshipBackgroundWorker.RunWorkerCompleted+=new RunWorkerCompletedEventHandler(_getRelationshipBackgroundWorker_RunWorkerCompleted);
            _getRelationshipBackgroundWorker.RunWorkerAsync();

            _getJobBackgroundWorker = new BackgroundWorker();
            _getJobBackgroundWorker.DoWork += new DoWorkEventHandler(_getJobBackgroundWorker_DoWork);
            _getJobBackgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(_getJobBackgroundWorker_RunWorkerCompleted);
            _getJobBackgroundWorker.RunWorkerAsync();
            Disposed += new EventHandler(ParentInfoPalmerwormItem_Disposed);
        }

        void ParentInfoPalmerwormItem_Disposed(object sender, EventArgs e)
        {
            JHParent.AfterUpdate -= new EventHandler<K12.Data.DataChangedEventArgs>(JHParent_AfterUpdate);
        }

        void JHParent_AfterUpdate(object sender, K12.Data.DataChangedEventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<object, K12.Data.DataChangedEventArgs>(JHParent_AfterUpdate), sender, e);
            }
            else
            {
                if (PrimaryKey != "")
                {
                    if (!BGWorker.IsBusy)
                        BGWorker.RunWorkerAsync();
                }
            }
        }

        void _getJobBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            //DSXmlHelper helper = (e.Result as DSResponse).GetContent();

            //foreach (XmlNode node in helper.GetElements("Job"))
            //{
            //    cboJob.Items.Add(new KeyValuePair<string, string>(node.InnerText, node.InnerText));
            //}
            
            // 職業
            foreach (string str in Utility.GetJobList())
                cboJob.Items.Add(new KeyValuePair<string, string>(str, str));
        }

        void _getJobBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            //e.Result = Config.GetJobList();
        }

        void _getNationalityBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            //DSXmlHelper helper = (e.Result as DSResponse).GetContent();

            //foreach (XmlNode node in helper.GetElements("Nationality"))
            //{
            //    cboNationality.Items.Add(new KeyValuePair<string, string>(node.InnerText, node.InnerText));
            //}
            
            //國籍
            foreach (string str in Utility.GetNationalityList())
                cboNationality.Items.Add(new KeyValuePair<string, string>(str,str));
        }

        void _getNationalityBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            //e.Result = Config.GetNationalityList();
        }

        void _getEduDegreeBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            //DSXmlHelper helper = (e.Result as DSResponse).GetContent();

            //foreach (XmlNode node in helper.GetElements("EducationDegree"))
            //{
            //    cboEduDegree.Items.Add(new KeyValuePair<string, string>(node.InnerText, node.InnerText));
            //}
            
            // 最高學歷
            foreach(string str in Utility.GetEducationDegreeList())
                cboEduDegree.Items.Add(new KeyValuePair<string, string>(str,str));
        }

        void _getEduDegreeBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            //e.Result = Config.GetEduDegreeList();
        }

        void BGWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (isBGBusy)
            {
                isBGBusy = false;
                BGWorker.RunWorkerAsync();
                return;
            }
            Initialize();
            BindDataToForm();
        }

        void BGWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            _StudParentRec = JHParent.SelectByStudentID(PrimaryKey);
        }

        protected override void OnPrimaryKeyChanged(EventArgs e)
        {
            this.Loading = true;
            if (BGWorker.IsBusy)
                isBGBusy = true;
            else
                BGWorker.RunWorkerAsync();
        }

        private void BindDataToForm()
        {   
            // Log
            prlp.SetBeforeSaveText("父親姓名",_StudParentRec.Father.Name);
            prlp.SetBeforeSaveText("父親身分證號",_StudParentRec.Father.IDNumber );
            prlp.SetBeforeSaveText("父親電話",_StudParentRec.Father.Phone );
            prlp.SetBeforeSaveText("父親存歿",_StudParentRec.Father.Living );
            prlp.SetBeforeSaveText("父親國籍",_StudParentRec.Father.Nationality );
            prlp.SetBeforeSaveText("父親職業",_StudParentRec.Father.Job );
            prlp.SetBeforeSaveText("父親最高學歷",_StudParentRec.Father.EducationDegree );
            prlp.SetBeforeSaveText("父親電子郵件", _StudParentRec.Father.EMail);
            prlp.SetBeforeSaveText("母親姓名",_StudParentRec.Mother.Name );
            prlp.SetBeforeSaveText("母親身分證號",_StudParentRec.Mother.IDNumber );
            prlp.SetBeforeSaveText("母親電話",_StudParentRec.Mother.Phone);
            prlp.SetBeforeSaveText("母親存歿",_StudParentRec.Mother.Living);
            prlp.SetBeforeSaveText("母親國籍",_StudParentRec.Mother.Nationality);
            prlp.SetBeforeSaveText("母親職業",_StudParentRec.Mother.Job);
            prlp.SetBeforeSaveText("母親最高學歷",_StudParentRec.Mother.EducationDegree);
            prlp.SetBeforeSaveText("母親電子郵件", _StudParentRec.Mother.EMail);
            prlp.SetBeforeSaveText("監護人姓名",_StudParentRec.Custodian.Name );
            prlp.SetBeforeSaveText("監護人身分證號",_StudParentRec.Custodian.IDNumber );
            prlp.SetBeforeSaveText("監護人電話",_StudParentRec.Custodian.Phone );
            prlp.SetBeforeSaveText("監護人關係",_StudParentRec.Custodian.Relationship);
            prlp.SetBeforeSaveText("監護人國籍",_StudParentRec.Custodian.Nationality );
            prlp.SetBeforeSaveText("監護人職業",_StudParentRec.Custodian.Job );
            prlp.SetBeforeSaveText("監護人最高學歷", _StudParentRec.Custodian.EducationDegree);
            prlp.SetBeforeSaveText("監護人電子郵件", _StudParentRec.Custodian.EMail);
            

            this.Loading = false;
            SaveButtonVisible = false;
            CancelButtonVisible = false;
            LoadDALDefaultData();

        
        }

        // 載入 DAL 預設取到值
        private void LoadDALDefaultData()
        {
            //DataListenerPause();
            //// 初始化
            //txtIDNumber.Text = "";
            //txtParentName.Text = "";
            //txtParentPhone.Text = "";
            //cboAlive.Text = "";
            //cboEduDegree.Text = "";
            //cboJob.Text = "";
            //cboNationality.Text = "";
            //cboRelationship.Text = "";

            if (_ParentType == ParentType.Guardian)
            {
                LoadGuardian();
            }

            if (_ParentType == ParentType.Father)
            {
                LoadFather();
            }

            if (_ParentType == ParentType.Mother)
            {
                LoadMother();
            }        
        }

        private void DataListenerPause()
        {
            _DataListener_Father.SuspendListen();
            _DataListener_Mother.SuspendListen();
            _DataListener_Guardian.SuspendListen();
        }

        private void btnGuardian_Click(object sender, EventArgs e)
        {
            LoadGuardian();
            _ParentType = ParentType.Guardian;
        }

        protected override void OnCancelButtonClick(EventArgs e)
        {
            SaveButtonVisible = false;
            CancelButtonVisible = false;
            LoadDALDefaultData();
        }

        protected override void OnSaveButtonClick(EventArgs e)
        {
            // 回存資料
            if (_ParentType == ParentType.Guardian)
            {
                _StudParentRec.Custodian.EducationDegree = cboEduDegree.Text;
                _StudParentRec.Custodian.IDNumber = txtIDNumber.Text;
                _StudParentRec.Custodian.Job = cboJob.Text;
                _StudParentRec.Custodian.Name = txtParentName.Text;
                _StudParentRec.Custodian.Nationality = cboNationality.Text;
                _StudParentRec.Custodian.Phone = txtParentPhone.Text;
                _StudParentRec.Custodian.Relationship = cboRelationship.Text;
                _StudParentRec.Custodian.EMail = txtEMail.Text;
            }

            if (_ParentType == ParentType.Father)
            {
                _StudParentRec.Father.EducationDegree = cboEduDegree.Text;
                _StudParentRec.Father.IDNumber = txtIDNumber.Text;
                _StudParentRec.Father.Job = cboJob.Text;
                _StudParentRec.Father.Living = cboAlive.Text;
                _StudParentRec.Father.Name = txtParentName.Text;
                _StudParentRec.Father.Nationality = cboNationality.Text;
                _StudParentRec.Father.Phone = txtParentPhone.Text;
                _StudParentRec.Father.EMail = txtEMail.Text;
            }

            if (_ParentType == ParentType.Mother)
            {
                _StudParentRec.Mother.EducationDegree = cboEduDegree.Text;
                _StudParentRec.Mother.IDNumber = txtIDNumber.Text;
                _StudParentRec.Mother.Job = cboJob.Text;
                _StudParentRec.Mother.Living = cboAlive.Text;
                _StudParentRec.Mother.Name = txtParentName.Text;
                _StudParentRec.Mother.Nationality = cboNationality.Text;
                _StudParentRec.Mother.Phone = txtParentPhone.Text;
                _StudParentRec.Mother.EMail = txtEMail.Text;
            }

            prlp.SetAfterSaveText("父親姓名", _StudParentRec.Father.Name);
            prlp.SetAfterSaveText("父親身分證號", _StudParentRec.Father.IDNumber);
            prlp.SetAfterSaveText("父親電話", _StudParentRec.Father.Phone);
            prlp.SetAfterSaveText("父親存歿", _StudParentRec.Father.Living);
            prlp.SetAfterSaveText("父親國籍", _StudParentRec.Father.Nationality);
            prlp.SetAfterSaveText("父親職業", _StudParentRec.Father.Job);
            prlp.SetAfterSaveText("父親最高學歷", _StudParentRec.Father.EducationDegree);
            prlp.SetAfterSaveText("父親電子郵件", _StudParentRec.Father.EMail);
            prlp.SetAfterSaveText("母親姓名", _StudParentRec.Mother.Name);
            prlp.SetAfterSaveText("母親身分證號", _StudParentRec.Mother.IDNumber);
            prlp.SetAfterSaveText("母親電話", _StudParentRec.Mother.Phone);
            prlp.SetAfterSaveText("母親存歿", _StudParentRec.Mother.Living);
            prlp.SetAfterSaveText("母親國籍", _StudParentRec.Mother.Nationality);
            prlp.SetAfterSaveText("母親職業", _StudParentRec.Mother.Job);
            prlp.SetAfterSaveText("母親最高學歷", _StudParentRec.Mother.EducationDegree);
            prlp.SetAfterSaveText("母親電子郵件", _StudParentRec.Mother.EMail);
            prlp.SetAfterSaveText("監護人姓名", _StudParentRec.Custodian.Name);
            prlp.SetAfterSaveText("監護人身分證號", _StudParentRec.Custodian.IDNumber);
            prlp.SetAfterSaveText("監護人電話", _StudParentRec.Custodian.Phone);
            prlp.SetAfterSaveText("監護人關係", _StudParentRec.Custodian.Relationship);
            prlp.SetAfterSaveText("監護人國籍", _StudParentRec.Custodian.Nationality);
            prlp.SetAfterSaveText("監護人職業", _StudParentRec.Custodian.Job);
            prlp.SetAfterSaveText("監護人最高學歷", _StudParentRec.Custodian.EducationDegree);
            prlp.SetAfterSaveText("監護人電子郵件", _StudParentRec.Custodian.EMail);
            JHParent.Update(_StudParentRec);
            prlp.SetActionBy("學籍", "學生父母及監護人資訊");
            prlp.SetAction("修改學生父母及監護人資訊");
            JHStudentRecord studRec = JHStudent.SelectByID(PrimaryKey);
            prlp.SetDescTitle("學生姓名:" + studRec.Name + ",學號:" + studRec.StudentNumber + ",");
            //Student.Instance.SyncDataBackground(PrimaryKey);
            Program.CustodianField.Reload();
            prlp.SaveLog("", "", "student", PrimaryKey);
            BindDataToForm();

        }

        void _DataListener_Mother_StatusChanged(object sender, ChangeEventArgs e)
        {
            if (Framework.User.Acl[GetType()].Editable)
                SaveButtonVisible = (e.Status == ValueStatus.Dirty);
            else
                SaveButtonVisible = false;

            CancelButtonVisible = (e.Status == ValueStatus.Dirty);
        }

        void _DataListener_Guardian_StatusChanged(object sender, ChangeEventArgs e)
        {
            if (Framework.User.Acl[GetType()].Editable)
                SaveButtonVisible = (e.Status == ValueStatus.Dirty);
            else
                SaveButtonVisible = false;

            CancelButtonVisible = (e.Status == ValueStatus.Dirty);
        }

        void _DataListener_Father_StatusChanged(object sender, ChangeEventArgs e)
        {
            if (Framework.User.Acl[GetType()].Editable)
                SaveButtonVisible = (e.Status == ValueStatus.Dirty);
            else
                SaveButtonVisible = false;

            CancelButtonVisible = (e.Status == ValueStatus.Dirty);
        }

        public DetailContent GetContent()
        {
            return new ParentInfoPalmerwormItem();
        }


        
        void _getRelationshipBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            //DSXmlHelper helper = (e.Result as DSResponse).GetContent();

            //foreach (XmlNode node in helper.GetElements("Relationship"))
            //{
            //    cboRelationship.Items.Add(new KeyValuePair<string, string>(node.InnerText, node.InnerText));
            //}

            // 稱謂
            foreach(string str in Utility.GetRelationshipList())
                cboRelationship.Items.Add(new KeyValuePair<string, string>(str, str));
        }

        void _getRelationshipBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            //e.Result = Config.GetRelationship();
        }
  
        private void Initialize()
        {
            if (_isInitialized) return;

            KeyValuePair<string, string> kvp = new KeyValuePair<string, string>("", "請選擇");
            cboRelationship.Items.Add(kvp);
            cboRelationship.DisplayMember = "Value";
            cboRelationship.ValueMember = "Key";

            cboJob.Items.Add(kvp);
            cboJob.DisplayMember = "Value";
            cboJob.ValueMember = "Key";


            cboEduDegree.Items.Add(kvp);
            cboEduDegree.DisplayMember = "Value";
            cboEduDegree.ValueMember = "Key";


            //載入國籍
            cboNationality.Items.Add(kvp);
            cboNationality.DisplayMember = "Value";
            cboNationality.ValueMember = "Key";

            //載入存殁
            kvp = new KeyValuePair<string, string>("存", "存");
            cboAlive.Items.Add(kvp);
            kvp = new KeyValuePair<string, string>("歿", "歿");
            cboAlive.Items.Add(kvp);
            cboAlive.DisplayMember = "Value";
            cboAlive.ValueMember = "Key";
            _isInitialized = true;
        }

        private void btnFather_Click(object sender, EventArgs e)
        {
            LoadFather();
            _ParentType = ParentType.Father;
        }

        private void btnMother_Click(object sender, EventArgs e)
        {
            LoadMother();
            _ParentType = ParentType.Mother;
        }

        // 載入監護人資料
        private void LoadGuardian()
        {
            DataListenerPause();
            btnGuardian.Enabled = false;
            btnFather.Enabled = true;
            btnMother.Enabled = true;

            cboAlive.Visible = false;
            lblAlive.Visible = false;
            cboRelationship.Visible = true;
            lblRelationship.Visible = true;

            btnParentType.Text = btnGuardian.Text;
            txtParentName.Text = _StudParentRec.Custodian.Name;
            txtIDNumber.Text = _StudParentRec.Custodian.IDNumber;
            txtParentPhone.Text = _StudParentRec.Custodian.Phone;
            cboRelationship.Text = _StudParentRec.Custodian.Relationship;
            cboNationality.Text = _StudParentRec.Custodian.Nationality;
            cboJob.Text = _StudParentRec.Custodian.Job;
            cboEduDegree.Text = _StudParentRec.Custodian.EducationDegree;
            txtEMail.Text = _StudParentRec.Custodian.EMail;
            //cboRelationship.SetComboBoxValue(_StudParentRec.Custodian.Relationship);
            //cboNationality.SetComboBoxValue(_StudParentRec.Custodian.Nationality);
            //cboJob.SetComboBoxValue(_StudParentRec.Custodian.Job);
            //cboEduDegree.SetComboBoxValue(_StudParentRec.Custodian.EducationDegree);

            lnkCopyGuardian.Visible = false;
            lnkCopyFather.Visible = true;
            lnkCopyMother.Visible = true;

            _DataListener_Guardian.Reset();
            _DataListener_Guardian.ResumeListen();
            
        }

        // 載入父親資料
        private void LoadFather()
        {
            DataListenerPause();
            btnGuardian.Enabled = true;
            btnFather.Enabled = false;
            btnMother.Enabled = true;

            cboAlive.Visible = true;
            lblAlive.Visible = true;
            cboRelationship.Visible = false;
            lblRelationship.Visible = false;

            btnParentType.Text = btnFather.Text;
            txtParentName.Text = _StudParentRec.Father.Name;
            txtIDNumber.Text = _StudParentRec.Father.IDNumber;
            txtParentPhone.Text = _StudParentRec.Father.Phone;

            cboAlive.Text = _StudParentRec.Father.Living;
            cboNationality.Text = _StudParentRec.Father.Nationality;
            cboJob.Text = _StudParentRec.Father.Job;
            cboEduDegree.Text = _StudParentRec.Father.EducationDegree;
            txtEMail.Text = _StudParentRec.Father.EMail;
            //cboAlive.SetComboBoxValue(_StudParentRec.Father.Living);
            //cboNationality.SetComboBoxValue(_StudParentRec.Father.Nationality);
            //cboJob.SetComboBoxValue(_StudParentRec.Father.Job);
            //cboEduDegree.SetComboBoxValue(_StudParentRec.Father.EducationDegree);

            lnkCopyGuardian.Visible = true;
            lnkCopyFather.Visible = false;
            lnkCopyMother.Visible = false;
            _DataListener_Father.Reset();
            _DataListener_Father.ResumeListen();
        }

        // 載入母親資料
        private void LoadMother()
        {
            DataListenerPause();
            btnGuardian.Enabled = true;
            btnFather.Enabled = true;
            btnMother.Enabled = false;
            cboAlive.Visible = true;
            lblAlive.Visible = true;
            cboRelationship.Visible = false;
            lblRelationship.Visible = false;
            btnParentType.Text = btnMother.Text;
            txtParentName.Text = _StudParentRec.Mother.Name;
            txtIDNumber.Text = _StudParentRec.Mother.IDNumber; txtParentPhone.Text = _StudParentRec.Mother.Phone;

            cboAlive.Text = _StudParentRec.Mother.Living;
            cboNationality.Text = _StudParentRec.Mother.Nationality;
            cboJob.Text = _StudParentRec.Mother.Job;
            cboEduDegree.Text = _StudParentRec.Mother.EducationDegree;
            txtEMail.Text = _StudParentRec.Mother.EMail;
            //cboAlive.SetComboBoxValue(_StudParentRec.Mother.Living);
            //cboNationality.SetComboBoxValue(_StudParentRec.Mother.Nationality);
            //cboJob.SetComboBoxValue(_StudParentRec.Mother.Job);
            //cboEduDegree.SetComboBoxValue(_StudParentRec.Mother.EducationDegree);

            lnkCopyGuardian.Visible = true;
            lnkCopyFather.Visible = false;
            lnkCopyMother.Visible = false;
            _DataListener_Mother.Reset();
            _DataListener_Mother.ResumeListen();
        }

        private void lnkCopyGuardian_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            txtParentName.Text = _StudParentRec.Custodian.Name;
            txtIDNumber.Text = _StudParentRec.Custodian.IDNumber;
            cboNationality.SetComboBoxValue(_StudParentRec.Custodian.Nationality);
            cboJob.SetComboBoxValue(_StudParentRec.Custodian.Job );
            cboEduDegree.SetComboBoxValue(_StudParentRec.Custodian.EducationDegree);
            txtParentPhone.Text = _StudParentRec.Custodian.Phone;
            txtEMail.Text = _StudParentRec.Custodian.EMail;
        }

        
        private void lnkCopyFather_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            txtParentName.Text = _StudParentRec.Father.Name;
            txtIDNumber.Text = _StudParentRec.Father.IDNumber;
            cboNationality.SetComboBoxValue(_StudParentRec.Father.Nationality);
            cboJob.SetComboBoxValue(_StudParentRec.Father.Job);
            cboEduDegree.SetComboBoxValue(_StudParentRec.Father.EducationDegree);
            if (btnParentType.Text == "監護人")
                cboRelationship.SetComboBoxValue("父");
            txtParentPhone.Text = _StudParentRec.Father.Phone;
            txtEMail.Text = _StudParentRec.Father.EMail;
        }

        private void lnkCopyMother_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            txtParentName.Text = _StudParentRec.Mother.Name;
            txtIDNumber.Text = _StudParentRec.Mother.IDNumber;
            cboNationality.SetComboBoxValue(_StudParentRec.Mother.Nationality);
            cboJob.SetComboBoxValue(_StudParentRec.Mother.Job);
            cboEduDegree.SetComboBoxValue(_StudParentRec.Mother.EducationDegree);
            if (btnParentType.Text == "監護人")
                cboRelationship.SetComboBoxValue("母");
            txtParentPhone.Text = _StudParentRec.Mother.Phone;
            txtEMail.Text = _StudParentRec.Mother.EMail;
        }
    
    }
}
