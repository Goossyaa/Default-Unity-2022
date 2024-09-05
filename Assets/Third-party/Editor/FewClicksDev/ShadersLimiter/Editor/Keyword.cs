namespace FewClicksDev.ShadersLimiter
{
    using UnityEngine;
    using UnityEngine.Rendering;

    [System.Serializable]
    public class Keyword
    {
        [SerializeField] private string keywordName = string.Empty;
        [SerializeField] private bool isStripped = false;

        public string KeywordName => keywordName;
        public bool IsStripped => isStripped;

        public bool GloballyStripped => ShadersLimiterDatabase.IsKeywordGloballyStripped(keywordName);
        public bool StrippedGloballyOrLocally => isStripped || GloballyStripped;

        public Keyword(ShaderKeyword _keyword, Shader _shaderReference)
        {
            keywordName = _keyword.GetNameWithShader(_shaderReference);
            isStripped = false;
        }

        public void SetAsStripped(bool _isStripped)
        {
            isStripped = _isStripped;
            ShadersLimiterDatabase.SetAsDirty();
        }

        public Color GetIndicatorColor()
        {
            return ShadersLimiterDatabase.GetIndicatorColor(this);
        }

        public override bool Equals(object _obj)
        {
            if (_obj is null || GetType().Equals(_obj.GetType()) == false)
            {
                return false;
            }

            Keyword _key = (Keyword) _obj;

            return keywordName == _key.KeywordName;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}