namespace FewClicksDev.ShadersLimiter
{
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    [CreateAssetMenu(menuName = "FewClicks Dev/Shaders Limiter/Keyword Description", fileName = "keywordDescription_KeywordName")]
    public class KeywordDescription : ScriptableObject
    {
        [System.Serializable]
        public struct LinkWithTitle
        {
            public string Title;
            public string DocumentationLink;
        }

        [SerializeField] private string keywordName = string.Empty;
        [SerializeField, TextArea(2, 5)] private string description = string.Empty;

        [Space]
        [SerializeField] private bool showAdditionalMessage = false;
        [SerializeField] private MessageType additionalMessageType = MessageType.Info;
        [SerializeField, TextArea(2, 5)] private string additionalMessage = string.Empty;

        [Space]
        [SerializeField] private List<LinkWithTitle> documentationLinks = new List<LinkWithTitle>();

        public string KeywordName => keywordName;
        public string Description => description;

        public bool ShowAdditionalMessage => showAdditionalMessage;
        public MessageType AdditionalMessageType => additionalMessageType;
        public string AdditionalMessage => additionalMessage;

        public List<LinkWithTitle> DocumentationLinks => documentationLinks;

        private void Awake()
        {
            RegisterToDatabase();
        }

        public void Setup(string _keyword)
        {
            keywordName = _keyword;
            EditorUtility.SetDirty(this);
        }

        public void RegisterToDatabase()
        {
            ShadersLimiterDatabase.RegisterToShaderKeywordsDatabase(this);
        }
    }
}