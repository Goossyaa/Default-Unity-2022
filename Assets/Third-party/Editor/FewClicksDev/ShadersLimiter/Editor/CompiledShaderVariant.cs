namespace FewClicksDev.ShadersLimiter
{
    using FewClicksDev.Core;
    using UnityEditor.Rendering;
    using UnityEngine;
    using UnityEngine.Rendering;

    [System.Serializable]
    public class CompiledShaderVariant : System.IEquatable<CompiledShaderVariant>
    {
        private const string SPACE = " ";
        private const char SPACE_CHAR = ' ';

        [SerializeField] private Shader shader = null;
        [SerializeField] private PassType shaderPass = PassType.Normal;
        [SerializeField] private string[] keywordsSet = null;
        [SerializeField] private string keywordsAsSingleString = string.Empty;
        [SerializeField] private int numberOfKeywords = 0;

        public Shader ShaderReference => shader;
        public PassType ShaderPass => shaderPass;
        public string KeywordsSetAsSingleString => keywordsAsSingleString;
        public string[] KeywordsSet => keywordsSet;
        public int NumberOfKeywords => numberOfKeywords;

        public CompiledShaderVariant(Shader _shader, ShaderSnippetData _snippet, ShaderKeyword[] _allKeywords)
        {
            shader = _shader;
            shaderPass = _snippet.passType;

            for (int i = 0; i < _allKeywords.Length; i++)
            {
                keywordsAsSingleString += _allKeywords[i].GetNameWithShader(_shader) + SPACE;
            }

            keywordsAsSingleString = keywordsAsSingleString.Trim();
            createStrings();
        }

        public CompiledShaderVariant(Shader _shader, ShaderVariantCollection.ShaderVariant _variant)
        {
            shader = _shader;
            shaderPass = _variant.passType;
            keywordsAsSingleString = string.Join(SPACE, _variant.keywords).Trim();
            createStrings();
        }

        public bool Equals(CompiledShaderVariant _other)
        {
            if (shader != _other.shader)
            {
                return false;
            }

            if (keywordsSet.Length != _other.keywordsSet.Length)
            {
                return false;
            }

            for (int i = 0; i < keywordsSet.Length; i++)
            {
                bool _anyMatch = false;

                for (int j = 0; j < _other.keywordsSet.Length; j++)
                {
                    if (keywordsSet[i].Trim() == _other.keywordsSet[j].Trim())
                    {
                        _anyMatch = true;
                        break;
                    }
                }

                if (_anyMatch == false)
                {
                    return false;
                }
            }

            return true;
        }

        public bool Contains(string _keywordName)
        {
            foreach (var _keyword in keywordsSet)
            {
                if (_keyword == _keywordName)
                {
                    return true;
                }
            }

            return false;
        }

        private void createStrings()
        {
            keywordsSet = keywordsAsSingleString.Split(SPACE_CHAR);

            if (keywordsAsSingleString.IsNullEmptyOrWhitespace())
            {
                numberOfKeywords = 0;
                keywordsAsSingleString = string.Empty;
                keywordsSet = new string[] { };
            }
            else
            {
                numberOfKeywords = keywordsSet.Length;
            }
        }
    }
}
