namespace FewClicksDev.ShadersLimiter
{
    using FewClicksDev.Core;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.Events;
    using UnityEngine.Rendering;

    [System.Serializable]
    public class ShaderToStrip
    {
        [System.Serializable]
        public class ShaderVariantData
        {
            [SerializeField] private int numberOfKeywords = 0;
            [SerializeField] private int startIndex = -1;
            [SerializeField] private int numberOfVariants = 0;

            public int NumberOfKeywords => numberOfKeywords;
            public int StartIndex => startIndex;
            public int NumberOfVariants => numberOfVariants;

            public ShaderVariantData(int _numberOfKeywords, int _startIndex)
            {
                numberOfKeywords = _numberOfKeywords;
                startIndex = _startIndex;

                numberOfVariants = 1;
            }

            public void AddVariant()
            {
                numberOfVariants++;
            }
        }

        public static event UnityAction<ShaderToStrip> OnExpandStateChanged = null;

        private const string HIDDEN_SHADERS = "Hidden";
        private const string LEGACY_SHADERS = "Legacy Shaders";

        [SerializeField] private Shader shaderReference = null;
        [SerializeField] private string shaderShortName = string.Empty;
        [SerializeField] private string shaderPath = string.Empty;
        [SerializeField] private ShaderType shaderType = ShaderType.Default;

        [SerializeField] private bool stripWholeShader = false;
        [SerializeField] private List<Keyword> availableKeywords = new List<Keyword>();

        [SerializeField] private bool compileOnlySpecifiedVariants = false;
        [SerializeField] private List<ShaderVariantCollection> variantsCollections = new List<ShaderVariantCollection>();
        [SerializeField] private List<CompiledShaderVariant> availableVariants = new List<CompiledShaderVariant>();

        [SerializeField] private List<ShaderVariantData> variantsOrderData = new List<ShaderVariantData>();

        [SerializeField] private bool forcePrintLogsOnCompile = false;
        [SerializeField] private bool forcePrintLogsOnStrip = false;

        public Shader ShaderReference => shaderReference;
        public string ShortName => shaderShortName;
        public string Path => shaderPath;

        public string ShaderPathToLower
        {
            get
            {
                if (shaderPathToLower == string.Empty)
                {
                    shaderPathToLower = shaderPath.ToLower();
                }

                return shaderPathToLower;
            }
        }

        private string shaderPathToLower = string.Empty;

        public ShaderType CurrentShaderType => shaderType;

        public bool IsStripped => stripWholeShader;
        public List<Keyword> AvailableKeywords => availableKeywords;
        public int NumberOfKeywords => availableKeywords.Count;

        public bool CompileOnlySpecifiedVariants => compileOnlySpecifiedVariants;
        public bool CanCompileOnlySpecifiedVariants => compileOnlySpecifiedVariants && availableVariants.Count > 0;

        public List<ShaderVariantCollection> VariantsCollections => variantsCollections;
        public List<CompiledShaderVariant> AvailableVariants => availableVariants;

        public bool ForcePrintLogsOnCompile => forcePrintLogsOnCompile;
        public bool ForcePrintLogsOnStrip => forcePrintLogsOnStrip;

        public bool IsAnyKeywordStripped => isAnyKeywordStripped();
        public bool AnyKeywordAvailable => availableKeywords.Count > 0;
        public bool AllKeywordsStripped => AnyKeywordAvailable && availableKeywords.Count == GetNumberOfStrippedKeywords();

        public Color IndicatorColor => ShadersLimiterDatabase.GetIndicatorColor(this);
        public Vector2 ScrollPosition { get; set; } = Vector2.zero;

        public bool IsExpanded
        {
            get => isExpanded;

            set
            {
                isExpanded = value;
                OnExpandStateChanged?.Invoke(this);
            }
        }

        private bool isExpanded = false;

        public Vector2 StartPositionAndHeight => startPositionAndHeight;
        private Vector2 startPositionAndHeight = new Vector2(0f, ShadersLimiterWindow.SINGLE_LINE_HEIGHT);

        public ShaderToStrip(Shader _reference, ShaderKeyword[] _keywords)
        {
            shaderReference = _reference;
            shaderShortName = getShaderShortName(_reference.name);
            shaderPath = _reference.name;
            shaderType = getShaderType(shaderPath);
            availableKeywords = new List<Keyword>();

            UpdateKeywordsToStrip(_keywords);
        }

        public void SetAsStripped(bool _stripWholeShader)
        {
            stripWholeShader = _stripWholeShader;
            ShadersLimiterDatabase.SetAsDirty();
        }

        public void SetCompileOnlySpecified(bool _allowSpecified)
        {
            compileOnlySpecifiedVariants = _allowSpecified;
            RecalculateHeight();
            ShadersLimiterDatabase.SetAsDirty();
        }

        public void ClearShaderCache()
        {
            ShaderUtil.ClearCachedData(shaderReference);
        }

        public void SetPrintLogsOnCompile(bool _print)
        {
            forcePrintLogsOnCompile = _print;
            ShadersLimiterDatabase.SetAsDirty();
        }

        public void SetPrintLogsOnStrip(bool _print)
        {
            forcePrintLogsOnStrip = _print;
            ShadersLimiterDatabase.SetAsDirty();
        }

        public void AddCollection(ShaderVariantCollection _collection, bool _updateVariants = true)
        {
            RemoveCollectionsNullReferences();

            if (variantsCollections.Contains(_collection))
            {
                return;
            }

            variantsCollections.Add(_collection);
            SetCompileOnlySpecified(true);

            if (_updateVariants)
            {
                UpdateVariantsCollection(true);
            }
        }

        public void RemoveCollection(ShaderVariantCollection _collection)
        {
            if (variantsCollections.Contains(_collection) == false)
            {
                return;
            }

            RemoveCollectionsNullReferences();
            variantsCollections.Remove(_collection);
            UpdateVariantsCollection(true);
        }

        public void ClearAllCollections()
        {
            variantsCollections.Clear();
            UpdateVariantsCollection(true);
            SetAllKeywordsAsStripped(false);
        }

        public void UpdateKeywordsToStrip(ShaderKeyword[] _keywords)
        {
            for (int i = 0; i < _keywords.Length; i++)
            {
                bool _isAlreadyAdded = false;

                for (int j = 0; j < availableKeywords.Count; j++)
                {
                    if (availableKeywords[j].KeywordName.Equals(_keywords[i].GetNameWithShader(shaderReference)))
                    {
                        _isAlreadyAdded = true;
                        break;
                    }
                }

                if (_isAlreadyAdded == false)
                {
                    availableKeywords.Add(new Keyword(_keywords[i], ShaderReference));
                    ShadersLimiterDatabase.AddToUniqueKeywords(_keywords[i].GetNameWithShader(shaderReference));
                }
            }
        }

        public bool IsAnyShaderVariantAMatch(CompiledShaderVariant _compiledShader)
        {
            ShaderVariantData _data = getDataWithKeywords(_compiledShader.NumberOfKeywords);

            if (_data == null) //There are no variants with this keywords count
            {
                return false;
            }

            int _startIndex = _data.StartIndex;

            for (int i = _startIndex; i < _startIndex + _data.NumberOfVariants; i++)
            {
                if (availableVariants[i].Equals(_compiledShader))
                {
                    return true;
                }
            }

            return false;
        }

        public void CalculateOrderDataIfNecessary()
        {
            if (variantsOrderData.IsNullOrEmpty())
            {
                calculateShadersInOrder();
            }
        }

        public void SetAllKeywordsAsStripped(bool _stripped)
        {
            foreach (var _keyword in availableKeywords)
            {
                _keyword.SetAsStripped(_stripped);
            }
        }

        public void StripKeywordsNotIncludedInCollection()
        {
            if (variantsCollections.IsNullOrEmpty() || availableVariants.IsNullOrEmpty())
            {
                return;
            }

            SetAllKeywordsAsStripped(false);

            foreach (var _keyword in availableKeywords)
            {
                var _contains = false;

                foreach (var _variant in availableVariants)
                {
                    if (_variant.Contains(_keyword.KeywordName))
                    {
                        _contains = true;
                        break;
                    }
                }

                if (_contains == false)
                {
                    _keyword.SetAsStripped(true);
                }
            }
        }

        public void UpdateVariantsCollection(bool _clearVariants)
        {
            if (_clearVariants)
            {
                availableVariants.Clear();
            }

            int _newVariantsCount = 0;

            foreach (var _collection in VariantsCollections)
            {
                if (_collection == null)
                {
                    continue;
                }

                var _newVariants = ShadersLimiter.GetShaderVariantsInCollection(_collection, ShaderReference);

                foreach (var _newVariant in _newVariants)
                {
                    bool _added = availableVariants.AddUnique(_newVariant);

                    if (_added)
                    {
                        _newVariantsCount++;
                    }
                }
            }

            if (_newVariantsCount > 0)
            {
                if (_clearVariants == false)
                {
                    ShadersLimiter.Log($"Added {_newVariantsCount} new variants of the {ShortName} shader to the database.");
                }
                else
                {
                    ShadersLimiter.Log($"There are {_newVariantsCount} variants of the {ShortName} shader in the database.");
                }
            }

            availableVariants = availableVariants.OrderBy(variant => variant.NumberOfKeywords).ToList();
            calculateShadersInOrder();
            RecalculateHeight();

            ShadersLimiterDatabase.SetAsDirty();
        }

        public int GetNumberOfStrippedKeywords()
        {
            int _count = 0;

            foreach (var _keyword in AvailableKeywords)
            {
                if (_keyword.StrippedGloballyOrLocally)
                {
                    _count++;
                }
            }

            return _count;
        }

        public void RemoveCollectionsNullReferences()
        {
            for (int i = variantsCollections.Count - 1; i >= 0; i--)
            {
                if (variantsCollections[i] == null)
                {
                    variantsCollections.RemoveAt(i);
                }

                if (AssetDatabase.Contains(variantsCollections[i]) == false)
                {
                    variantsCollections.RemoveAt(i);
                }
            }
        }

        public void RecalculateHeight()
        {
            RecalculateHeightAndStartPosition(StartPositionAndHeight.x);
        }

        public void RecalculateHeightAndStartPosition(float _currentY)
        {
            startPositionAndHeight.x = _currentY;

            float _height = ShadersLimiterWindow.SINGLE_LINE_HEIGHT;

            if (IsExpanded)
            {
                _height += EditorDrawer.SMALL_SPACE;
                _height += EditorDrawer.SingleLineHeightWithSpacing;

                if (AnyKeywordAvailable == false)
                {
                    _height += EditorDrawer.SMALL_SPACE;
                }
                else
                {
                    _height += EditorDrawer.SMALL_SPACE + 1f;
                    _height += EditorDrawer.SingleLineHeight;
                    _height += ShadersLimiterWindow.SINGLE_LINE_HEIGHT; //Helper buttons
                    _height += EditorDrawer.NORMAL_SPACE;
                    _height += EditorDrawer.SingleLineHeight; //Conditional compilation label
                    _height += EditorDrawer.SMALL_SPACE;
                    _height += EditorDrawer.SingleLineHeight; //Compile only specified

                    if (compileOnlySpecifiedVariants)
                    {
                        int _numberOfCollections = variantsCollections.Count;

                        if (_numberOfCollections < 1)
                        {
                            _height += EditorDrawer.HELP_BOX_HEIGHT; //No collections
                        }
                        else
                        {
                            _height += _numberOfCollections * ShadersLimiterWindow.BUTTON_HEIGHT;
                            _height += EditorDrawer.SMALL_SPACE;
                            _height += ShadersLimiterWindow.KEYWORD_HEIGHT; //Add new collection button

                            if (AvailableVariants.Count > 0)
                            {
                                _height += EditorDrawer.SMALL_SPACE;
                                _height += ShadersLimiterWindow.BUTTON_HEIGHT; //Helper buttons e.g. Show available variants
                            }
                            else
                            {
                                _height += EditorDrawer.SMALL_SPACE;
                                _height += EditorDrawer.HELP_BOX_HEIGHT; //No variants in the attached collection
                            }
                        }
                    }

                    _height += EditorDrawer.NORMAL_SPACE;
                    _height += EditorDrawer.SingleLineHeightWithSpacing; //Keywords label
                    _height += Mathf.Clamp(NumberOfKeywords, 0, ShadersLimiterDatabase.MaxNumberVisibleKeywords) * ShadersLimiterWindow.KEYWORD_HEIGHT;
                    _height += EditorDrawer.LARGE_SPACE;
                }
            }

            startPositionAndHeight.y = _height;
        }

        private void calculateShadersInOrder()
        {
            if (variantsOrderData == null)
            {
                variantsOrderData = new List<ShaderVariantData>();
            }

            variantsOrderData.Clear();

            int _index = 0;

            foreach (var _variant in availableVariants)
            {
                int _numberOfKeywords = _variant.NumberOfKeywords;
                ShaderVariantData _data = getDataWithKeywords(_numberOfKeywords);

                if (_data == null)
                {
                    _data = new ShaderVariantData(_numberOfKeywords, _index);
                    variantsOrderData.Add(_data);
                }
                else
                {
                    _data.AddVariant();
                }

                _index++;
            }

            ShadersLimiterDatabase.SetAsDirty();
        }

        private ShaderVariantData getDataWithKeywords(int _numberOfKeywords)
        {
            foreach (var _data in variantsOrderData)
            {
                if (_data.NumberOfKeywords == _numberOfKeywords)
                {
                    return _data;
                }
            }

            return null;
        }

        private bool isAnyKeywordStripped()
        {
            for (int i = 0; i < availableKeywords.Count; i++)
            {
                if (availableKeywords[i].StrippedGloballyOrLocally)
                {
                    return true;
                }
            }

            return false;
        }

        private static string getShaderShortName(string _path)
        {
            string[] _split = _path.Split('/');
            return _split[_split.Length - 1];
        }

        private static ShaderType getShaderType(string _shaderPath)
        {
            if (_shaderPath.StartsWith(HIDDEN_SHADERS))
            {
                return ShaderType.Hidden;
            }
            else if (_shaderPath.StartsWith(LEGACY_SHADERS))
            {
                return ShaderType.Legacy;
            }

            return ShaderType.Default;
        }
    }
}
