/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

namespace Contoso
{
    namespace RetailServer.TestClient
    {
        internal partial class MainForm
        {
            private System.Windows.Forms.GroupBox groupBoxActivation;
            private System.Windows.Forms.Button btnActivateNew;
            private System.Windows.Forms.ListView listViewActivationDetails;
            private System.Windows.Forms.Button btnLogin;
            private System.Windows.Forms.GroupBox groupBox1;
            private System.Windows.Forms.Button btnDefaultTests;
            private System.Windows.Forms.GroupBox groupBoxAnonymous;
            private System.Windows.Forms.Button btnLogOff;
            private System.Windows.Forms.Button btnDeactivate;
            private System.Windows.Forms.Button btnShowDebugWindow;
            private System.Windows.Forms.Button btnClearDebug;
            private System.Windows.Forms.Button btnSdkSampleTests;
            private System.Windows.Forms.Button btnExtensionTests;
            private System.Windows.Forms.CheckBox chkCallWithDeviceToken;
            private System.Windows.Forms.CheckBox chkCallWithUserToken;
            private System.Windows.Forms.CheckBox chkCallWithOperatingUnitNumber;
            private System.Windows.Forms.TextBox txtOperatingUnitNumber;
            private System.Windows.Forms.ComboBox comboRetailServerUrl;
            private System.Windows.Forms.Button btnSelectActivated;
            private System.Windows.Forms.TextBox txtNewRetailServerUrl;
            private System.Windows.Forms.ListView listViewLogonDetails;
            private System.Windows.Forms.Label label1;
            private System.Windows.Forms.CheckBox chkUseOnlineMode;
            private System.Windows.Forms.Button btnTestOfflineAdapters;

            /// <summary>
            /// Required designer variable.
            /// </summary>
            private System.ComponentModel.IContainer components = null;

            /// <summary>
            /// Clean up any resources being used.
            /// </summary>
            /// <param name="disposing">True if managed resources should be disposed; otherwise, false.</param>
            protected override void Dispose(bool disposing)
            {
                if (disposing && (this.components != null))
                {
                    this.components.Dispose();
                }

                base.Dispose(disposing);
            }

            #region Windows Form Designer generated code

            /// <summary>
            /// Required method for Designer support - do not modify
            /// the contents of this method with the code editor.
            /// </summary>
            private void InitializeComponent()
            {
            this.groupBoxActivation = new System.Windows.Forms.GroupBox();
            this.chkUseOnlineMode = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btnActivateNew = new System.Windows.Forms.Button();
            this.txtNewRetailServerUrl = new System.Windows.Forms.TextBox();
            this.btnDeactivate = new System.Windows.Forms.Button();
            this.btnSelectActivated = new System.Windows.Forms.Button();
            this.listViewActivationDetails = new System.Windows.Forms.ListView();
            this.comboRetailServerUrl = new System.Windows.Forms.ComboBox();
            this.txtOperatingUnitNumber = new System.Windows.Forms.TextBox();
            this.chkCallWithOperatingUnitNumber = new System.Windows.Forms.CheckBox();
            this.chkCallWithUserToken = new System.Windows.Forms.CheckBox();
            this.chkCallWithDeviceToken = new System.Windows.Forms.CheckBox();
            this.btnClearDebug = new System.Windows.Forms.Button();
            this.btnShowDebugWindow = new System.Windows.Forms.Button();
            this.btnLogin = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.listViewLogonDetails = new System.Windows.Forms.ListView();
            this.btnLogOff = new System.Windows.Forms.Button();
            this.btnDefaultTests = new System.Windows.Forms.Button();
            this.groupBoxAnonymous = new System.Windows.Forms.GroupBox();
            this.btnExtensionTests = new System.Windows.Forms.Button();
            this.btnSdkSampleTests = new System.Windows.Forms.Button();
            this.btnTestOfflineAdapters = new System.Windows.Forms.Button();
            this.groupBoxActivation.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBoxAnonymous.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBoxActivation
            // 
            this.groupBoxActivation.Controls.Add(this.chkUseOnlineMode);
            this.groupBoxActivation.Controls.Add(this.label1);
            this.groupBoxActivation.Controls.Add(this.btnActivateNew);
            this.groupBoxActivation.Controls.Add(this.txtNewRetailServerUrl);
            this.groupBoxActivation.Controls.Add(this.btnDeactivate);
            this.groupBoxActivation.Controls.Add(this.btnSelectActivated);
            this.groupBoxActivation.Controls.Add(this.listViewActivationDetails);
            this.groupBoxActivation.Controls.Add(this.comboRetailServerUrl);
            this.groupBoxActivation.Location = new System.Drawing.Point(12, 4);
            this.groupBoxActivation.Name = "groupBoxActivation";
            this.groupBoxActivation.Size = new System.Drawing.Size(705, 237);
            this.groupBoxActivation.TabIndex = 0;
            this.groupBoxActivation.TabStop = false;
            this.groupBoxActivation.Text = "Retailserver / Device details";
            // 
            // chkUseOnlineMode
            // 
            this.chkUseOnlineMode.AutoSize = true;
            this.chkUseOnlineMode.Location = new System.Drawing.Point(6, 200);
            this.chkUseOnlineMode.Name = "chkUseOnlineMode";
            this.chkUseOnlineMode.Size = new System.Drawing.Size(152, 17);
            this.chkUseOnlineMode.TabIndex = 11;
            this.chkUseOnlineMode.Text = "Online mode (RetailServer)";
            this.chkUseOnlineMode.UseVisualStyleBackColor = true;
            this.chkUseOnlineMode.CheckedChanged += new System.EventHandler(this.chkUseOnlineMode_CheckedChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 183);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(297, 13);
            this.label1.TabIndex = 10;
            this.label1.Text = "Note: Do not include \"Commerce\" or \"$metadata\" in the URL";
            // 
            // btnActivateNew
            // 
            this.btnActivateNew.Location = new System.Drawing.Point(572, 161);
            this.btnActivateNew.Name = "btnActivateNew";
            this.btnActivateNew.Size = new System.Drawing.Size(118, 23);
            this.btnActivateNew.TabIndex = 0;
            this.btnActivateNew.Text = "Activate New";
            this.btnActivateNew.UseVisualStyleBackColor = true;
            this.btnActivateNew.Click += new System.EventHandler(this.btnActivate_Click);
            // 
            // txtNewRetailServerUrl
            // 
            this.txtNewRetailServerUrl.Location = new System.Drawing.Point(6, 161);
            this.txtNewRetailServerUrl.Name = "txtNewRetailServerUrl";
            this.txtNewRetailServerUrl.Size = new System.Drawing.Size(560, 20);
            this.txtNewRetailServerUrl.TabIndex = 9;
            this.txtNewRetailServerUrl.Text = "https://usnconeboxax1ret.cloud.onebox.dynamics.com/RetailServer";
            // 
            // btnDeactivate
            // 
            this.btnDeactivate.Location = new System.Drawing.Point(572, 63);
            this.btnDeactivate.Name = "btnDeactivate";
            this.btnDeactivate.Size = new System.Drawing.Size(118, 23);
            this.btnDeactivate.TabIndex = 2;
            this.btnDeactivate.Text = "Deactivate Device";
            this.btnDeactivate.UseVisualStyleBackColor = true;
            this.btnDeactivate.Click += new System.EventHandler(this.btnDeactivate_Click);
            // 
            // btnSelectActivated
            // 
            this.btnSelectActivated.Enabled = false;
            this.btnSelectActivated.Location = new System.Drawing.Point(572, 134);
            this.btnSelectActivated.Name = "btnSelectActivated";
            this.btnSelectActivated.Size = new System.Drawing.Size(118, 23);
            this.btnSelectActivated.TabIndex = 8;
            this.btnSelectActivated.Text = "Select activated";
            this.btnSelectActivated.UseVisualStyleBackColor = true;
            this.btnSelectActivated.Click += new System.EventHandler(this.btnSelectActivated_Click);
            // 
            // listViewActivationDetails
            // 
            this.listViewActivationDetails.Location = new System.Drawing.Point(6, 19);
            this.listViewActivationDetails.Name = "listViewActivationDetails";
            this.listViewActivationDetails.Size = new System.Drawing.Size(560, 109);
            this.listViewActivationDetails.TabIndex = 1;
            this.listViewActivationDetails.UseCompatibleStateImageBehavior = false;
            this.listViewActivationDetails.View = System.Windows.Forms.View.List;
            // 
            // comboRetailServerUrl
            // 
            this.comboRetailServerUrl.DisplayMember = "UniqueActivationId";
            this.comboRetailServerUrl.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboRetailServerUrl.FormattingEnabled = true;
            this.comboRetailServerUrl.Location = new System.Drawing.Point(6, 134);
            this.comboRetailServerUrl.Name = "comboRetailServerUrl";
            this.comboRetailServerUrl.Size = new System.Drawing.Size(560, 21);
            this.comboRetailServerUrl.TabIndex = 7;
            this.comboRetailServerUrl.SelectedIndexChanged += new System.EventHandler(this.comboRetailServerUrl_SelectedIndexChanged);
            // 
            // txtOperatingUnitNumber
            // 
            this.txtOperatingUnitNumber.Location = new System.Drawing.Point(23, 88);
            this.txtOperatingUnitNumber.Name = "txtOperatingUnitNumber";
            this.txtOperatingUnitNumber.Size = new System.Drawing.Size(139, 20);
            this.txtOperatingUnitNumber.TabIndex = 6;
            // 
            // chkCallWithOperatingUnitNumber
            // 
            this.chkCallWithOperatingUnitNumber.AutoSize = true;
            this.chkCallWithOperatingUnitNumber.Checked = true;
            this.chkCallWithOperatingUnitNumber.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkCallWithOperatingUnitNumber.Location = new System.Drawing.Point(6, 65);
            this.chkCallWithOperatingUnitNumber.Name = "chkCallWithOperatingUnitNumber";
            this.chkCallWithOperatingUnitNumber.Size = new System.Drawing.Size(170, 17);
            this.chkCallWithOperatingUnitNumber.TabIndex = 5;
            this.chkCallWithOperatingUnitNumber.Text = "Call with OperatingUnitNumber";
            this.chkCallWithOperatingUnitNumber.UseVisualStyleBackColor = true;
            this.chkCallWithOperatingUnitNumber.CheckedChanged += new System.EventHandler(this.CallWithToken_Changed);
            // 
            // chkCallWithUserToken
            // 
            this.chkCallWithUserToken.AutoSize = true;
            this.chkCallWithUserToken.Checked = true;
            this.chkCallWithUserToken.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkCallWithUserToken.Location = new System.Drawing.Point(6, 42);
            this.chkCallWithUserToken.Name = "chkCallWithUserToken";
            this.chkCallWithUserToken.Size = new System.Drawing.Size(120, 17);
            this.chkCallWithUserToken.TabIndex = 4;
            this.chkCallWithUserToken.Text = "Call with User token";
            this.chkCallWithUserToken.UseVisualStyleBackColor = true;
            // 
            // chkCallWithDeviceToken
            // 
            this.chkCallWithDeviceToken.AutoSize = true;
            this.chkCallWithDeviceToken.Checked = true;
            this.chkCallWithDeviceToken.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkCallWithDeviceToken.Location = new System.Drawing.Point(6, 19);
            this.chkCallWithDeviceToken.Name = "chkCallWithDeviceToken";
            this.chkCallWithDeviceToken.Size = new System.Drawing.Size(132, 17);
            this.chkCallWithDeviceToken.TabIndex = 3;
            this.chkCallWithDeviceToken.Text = "Call with Device token";
            this.chkCallWithDeviceToken.UseVisualStyleBackColor = true;
            this.chkCallWithDeviceToken.CheckedChanged += new System.EventHandler(this.CallWithToken_Changed);
            // 
            // btnClearDebug
            // 
            this.btnClearDebug.Location = new System.Drawing.Point(584, 527);
            this.btnClearDebug.Name = "btnClearDebug";
            this.btnClearDebug.Size = new System.Drawing.Size(118, 23);
            this.btnClearDebug.TabIndex = 4;
            this.btnClearDebug.Text = "Clear Debug";
            this.btnClearDebug.UseVisualStyleBackColor = true;
            this.btnClearDebug.Click += new System.EventHandler(this.btnClearDebug_Click);
            // 
            // btnShowDebugWindow
            // 
            this.btnShowDebugWindow.Location = new System.Drawing.Point(460, 527);
            this.btnShowDebugWindow.Name = "btnShowDebugWindow";
            this.btnShowDebugWindow.Size = new System.Drawing.Size(118, 23);
            this.btnShowDebugWindow.TabIndex = 3;
            this.btnShowDebugWindow.Text = "Debug";
            this.btnShowDebugWindow.UseVisualStyleBackColor = true;
            this.btnShowDebugWindow.Click += new System.EventHandler(this.btnShowDebugWindow_Click);
            // 
            // btnLogin
            // 
            this.btnLogin.Location = new System.Drawing.Point(572, 19);
            this.btnLogin.Name = "btnLogin";
            this.btnLogin.Size = new System.Drawing.Size(118, 23);
            this.btnLogin.TabIndex = 0;
            this.btnLogin.Text = "Logon";
            this.btnLogin.UseVisualStyleBackColor = true;
            this.btnLogin.Click += new System.EventHandler(this.btnLogin_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.listViewLogonDetails);
            this.groupBox1.Controls.Add(this.btnLogOff);
            this.groupBox1.Controls.Add(this.btnLogin);
            this.groupBox1.Location = new System.Drawing.Point(12, 280);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(705, 83);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Logon";
            // 
            // listViewLogonDetails
            // 
            this.listViewLogonDetails.Location = new System.Drawing.Point(6, 19);
            this.listViewLogonDetails.Name = "listViewLogonDetails";
            this.listViewLogonDetails.Size = new System.Drawing.Size(560, 52);
            this.listViewLogonDetails.TabIndex = 2;
            this.listViewLogonDetails.UseCompatibleStateImageBehavior = false;
            this.listViewLogonDetails.View = System.Windows.Forms.View.List;
            // 
            // btnLogOff
            // 
            this.btnLogOff.Location = new System.Drawing.Point(572, 48);
            this.btnLogOff.Name = "btnLogOff";
            this.btnLogOff.Size = new System.Drawing.Size(118, 23);
            this.btnLogOff.TabIndex = 1;
            this.btnLogOff.Text = "LogOff";
            this.btnLogOff.UseVisualStyleBackColor = true;
            this.btnLogOff.Click += new System.EventHandler(this.btnLogOff_Click);
            // 
            // btnDefaultTests
            // 
            this.btnDefaultTests.Location = new System.Drawing.Point(572, 15);
            this.btnDefaultTests.Name = "btnDefaultTests";
            this.btnDefaultTests.Size = new System.Drawing.Size(118, 23);
            this.btnDefaultTests.TabIndex = 2;
            this.btnDefaultTests.Text = "Default";
            this.btnDefaultTests.UseVisualStyleBackColor = true;
            this.btnDefaultTests.Click += new System.EventHandler(this.btnDefaultTests_Click);
            // 
            // groupBoxAnonymous
            // 
            this.groupBoxAnonymous.Controls.Add(this.txtOperatingUnitNumber);
            this.groupBoxAnonymous.Controls.Add(this.btnExtensionTests);
            this.groupBoxAnonymous.Controls.Add(this.chkCallWithOperatingUnitNumber);
            this.groupBoxAnonymous.Controls.Add(this.chkCallWithUserToken);
            this.groupBoxAnonymous.Controls.Add(this.btnSdkSampleTests);
            this.groupBoxAnonymous.Controls.Add(this.chkCallWithDeviceToken);
            this.groupBoxAnonymous.Controls.Add(this.btnDefaultTests);
            this.groupBoxAnonymous.Location = new System.Drawing.Point(12, 393);
            this.groupBoxAnonymous.Name = "groupBoxAnonymous";
            this.groupBoxAnonymous.Size = new System.Drawing.Size(705, 119);
            this.groupBoxAnonymous.TabIndex = 3;
            this.groupBoxAnonymous.TabStop = false;
            this.groupBoxAnonymous.Text = "Actions";
            // 
            // btnExtensionTests
            // 
            this.btnExtensionTests.Location = new System.Drawing.Point(572, 73);
            this.btnExtensionTests.Name = "btnExtensionTests";
            this.btnExtensionTests.Size = new System.Drawing.Size(118, 23);
            this.btnExtensionTests.TabIndex = 4;
            this.btnExtensionTests.Text = "Custom";
            this.btnExtensionTests.UseVisualStyleBackColor = true;
            this.btnExtensionTests.Click += new System.EventHandler(this.btnExtensionTests_Click);
            // 
            // btnSdkSampleTests
            // 
            this.btnSdkSampleTests.Location = new System.Drawing.Point(572, 44);
            this.btnSdkSampleTests.Name = "btnSdkSampleTests";
            this.btnSdkSampleTests.Size = new System.Drawing.Size(118, 23);
            this.btnSdkSampleTests.TabIndex = 3;
            this.btnSdkSampleTests.Text = "Sdk Tests";
            this.btnSdkSampleTests.UseVisualStyleBackColor = true;
            this.btnSdkSampleTests.Click += new System.EventHandler(this.btnSdkSampleTests_Click);
            // 
            // btnTestOfflineAdapters
            // 
            this.btnTestOfflineAdapters.Location = new System.Drawing.Point(12, 527);
            this.btnTestOfflineAdapters.Name = "btnTestOfflineAdapters";
            this.btnTestOfflineAdapters.Size = new System.Drawing.Size(118, 23);
            this.btnTestOfflineAdapters.TabIndex = 7;
            this.btnTestOfflineAdapters.Text = "Test Offline Adapters";
            this.btnTestOfflineAdapters.UseVisualStyleBackColor = true;
            this.btnTestOfflineAdapters.Click += new System.EventHandler(this.btnTestOfflineAdapters_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(729, 562);
            this.Controls.Add(this.btnTestOfflineAdapters);
            this.Controls.Add(this.btnClearDebug);
            this.Controls.Add(this.btnShowDebugWindow);
            this.Controls.Add(this.groupBoxAnonymous);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.groupBoxActivation);
            this.Name = "MainForm";
            this.Text = "TestClient";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.groupBoxActivation.ResumeLayout(false);
            this.groupBoxActivation.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBoxAnonymous.ResumeLayout(false);
            this.groupBoxAnonymous.PerformLayout();
            this.ResumeLayout(false);

            }

            #endregion
        }
    }
}
