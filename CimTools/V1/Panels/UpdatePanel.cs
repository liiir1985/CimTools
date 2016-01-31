﻿using CimTools.V1.Elements;
using CimTools.V1.Workshop;
using ColossalFramework.UI;
using UnityEngine;

namespace CimTools.V1.Panels
{
    /// <summary>
    /// A speech bubble which contains the latest changes to your mod.
    /// The bubble can be clicked and expanded to reveal new changes, rather than
    /// taking up loads of space.
    /// </summary>
    public class UpdatePanel : UIPanel
    {
        private RectOffset m_UIPadding = new RectOffset(5, 5, 5, 5);
        private UITitleBar m_panelTitle;
        private UILabel m_infoLabel;
        private CimToolBase m_toolBase = null;
        private bool showInitially = true;

        /// <summary>
        /// Set and get a Changelog downloader. By default this uses the default Changelog
        /// instance, but you can pass your own through here if you wish to use your own.
        /// <para>
        /// Please note that if you are using the default Changelog instance you will need to set
        /// it up for your mod, otherwise you'll get no changes.
        /// </para>
        /// </summary>
        public Changelog m_changelogDownloader = null;

        /// <summary>
        /// The initial message title when an update has been detected.
        /// </summary>
        public string m_updatedTitleMessage = "I've updated!";

        /// <summary>
        /// The initial message content when an update has been detected.
        /// </summary>
        public string m_updatedContentMessage = "<color#c8f582>Click here</color> to see what's changed";

        /// <summary>
        /// Automatically initialises the update panel with settings from
        /// the tool base.
        /// </summary>
        /// <param name="toolBase">Your internally saved CimToolBase</param>
        public void Initialise(CimToolBase toolBase)
        {
            m_changelogDownloader = toolBase.Changelog;
            m_updatedTitleMessage = string.Format("{0} has updated!", toolBase.ModSettings.ModName);

            string lastSavedVersion = "0.0";

            if(toolBase.XMLFileOptions.Data.GetValue("lastUpdatedVersion", ref lastSavedVersion, "UpdatePanel") == File.ExportOptionBase.OptionError.NoError)
            {
                showInitially = lastSavedVersion != toolBase.Version.Delimited(File.Version.Limit.Revision);
            }

            if(showInitially)
            {
                Show();
            }
            else
            {
                Hide();
            }

            m_toolBase = toolBase;
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Awake()
        {
            this.isInteractive = true;
            this.enabled = true;
            this.width = 300;
            this.height = 100;

            base.Awake();
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Start()
        {
            base.Start();

            m_panelTitle = this.AddUIComponent<UITitleBar>();
            m_panelTitle.title = m_updatedTitleMessage;

            m_infoLabel = this.AddUIComponent<UILabel>();
            m_infoLabel.width = this.width - m_UIPadding.left - m_UIPadding.right;
            m_infoLabel.wordWrap = true;
            m_infoLabel.processMarkup = true;
            m_infoLabel.autoHeight = true;
            m_infoLabel.text = m_updatedContentMessage;
            m_infoLabel.textScale = 0.6f;
            m_infoLabel.relativePosition = new Vector3(m_UIPadding.left, m_panelTitle.height + m_UIPadding.bottom);
            m_infoLabel.eventClicked += M_infoLabel_eventClicked;

            this.atlas = Utilities.UIUtilities.GetAtlas("Ingame");
            this.backgroundSprite = "InfoBubble";
            this.height = m_infoLabel.relativePosition.y + m_infoLabel.height + m_UIPadding.bottom + 20;
        }

        private void M_infoLabel_eventClicked(UIComponent component, UIMouseEventParameter eventParam)
        {
            ShowUpdateInfo();
        }

        /// <summary>
        /// Sets the position of the panel based on the "speaky point" of the
        /// speech bubble it creates
        /// </summary>
        /// <param name="position">Position to set the panel to</param>
        public void SetPositionSpeakyPoint(Vector2 position)
        {
            this.relativePosition = new Vector3(position.x, position.y - this.height);
        }

        private void ShowUpdateInfo()
        {
            float lastHeight = m_infoLabel.height;

            m_infoLabel.text = "Unable to retrieve the latest changes! Check on the workshop for the most recent changes.";

            if (m_changelogDownloader != null && !m_changelogDownloader.DownloadInProgress && !m_changelogDownloader.DownloadError)
            {
                m_infoLabel.text = m_changelogDownloader.ChangesString;
            }
            else
            {
                Debug.LogError("Failed to download workshop changes!");
            }

            float heightDifference = m_infoLabel.height - lastHeight;

            this.height = m_infoLabel.relativePosition.y + m_infoLabel.height + m_UIPadding.bottom + 20;
            this.relativePosition -= new Vector3(0, heightDifference);

            if(m_toolBase != null)
            {
                m_toolBase.XMLFileOptions.Data.SetValue("lastUpdatedVersion", m_toolBase.Version.Delimited(File.Version.Limit.Revision), "UpdatePanel");
                m_toolBase.XMLFileOptions.Save();
            }
        }
    }
}