namespace FewClicksDev.ShadersLimiter
{
    using FewClicksDev.Core;
    using System.Collections.Generic;
    using UnityEditor.Build;
    using UnityEditor.Build.Reporting;
    using UnityEditor.Rendering;
    using UnityEngine;
    using UnityEngine.Rendering;

    public class ShadersLimiterProcessor : IPreprocessShaders, IPostprocessBuildWithReport
    {
        public static bool PrintCompilingLogs => ShadersLimiterDatabase.CompiledShadersLogsEnabled;
        public static bool PrintStrippingLogs => ShadersLimiterDatabase.StrippedShaderLogsEnabled;

        public int callbackOrder => 0;

        public void OnPostprocessBuild(BuildReport _report)
        {
            if (ShadersLimiterDatabase.StrippingEnabledForFirstBuild)
            {
                //Delete shaders cache because it's all stripped anyway
                ShadersLimiterDatabase.DeleteBuildCache(true);
            }

            ShadersLimiterDatabase.FinishFirstBuild();
        }

        public void OnProcessShader(Shader _shader, ShaderSnippetData _snippet, IList<ShaderCompilerData> _data)
        {
            if (ShadersLimiterDatabase.DatabaseExists == false)
            {
                return;
            }

            ShaderToStrip _currentShader = ShadersLimiterDatabase.GetShaderToStrip(_shader); 

            for (int i = 0; i < _data.Count; i++)
            {
                ShadersLimiterDatabase.AddShaderToTheDatabase(_shader, _data[i].shaderKeywordSet.GetShaderKeywords());

                if (_currentShader == null)
                {
                    _currentShader = ShadersLimiterDatabase.GetShaderToStrip(_shader);
                }

                if (ShadersLimiterDatabase.StrippingEnabledForFirstBuild) // For first build only
                {
                    _data.RemoveAt(i);
                    --i;
                    continue;
                }

                if (ShadersLimiterDatabase.StrippingEnabled == false)
                {
                    continue;
                }

                if (_currentShader.IsStripped)
                {
                    bool _printLogs = PrintStrippingLogs || _currentShader.ForcePrintLogsOnStrip;

                    if (_printLogs)
                    {
                        ShadersLimiter.Log($"Stripping whole shader {_shader.name}.", true);
                    }

                    _data.RemoveAt(i);
                    --i;
                    continue;
                }

                var _allKeywords = _data[i].shaderKeywordSet.GetShaderKeywords();
                bool _stripVariant = stripShadersBasedOnStrippedKeywords(_currentShader, _snippet, _allKeywords);

                if (_stripVariant == false && _currentShader.CompileOnlySpecifiedVariants)
                {
                    _stripVariant = stripShadersBasedOnShadersAllowedToCompile(_currentShader, _snippet, _allKeywords);
                }

                if (_stripVariant)
                {
                    _data.RemoveAt(i);
                    --i;
                }
                else
                {
                    bool _printLogs = PrintCompilingLogs || _currentShader.ForcePrintLogsOnCompile;

                    if (_printLogs)
                    {
                        string _passName = _snippet.passType.ToString().InsertSpaceBeforeUpperCaseAndNumeric();

                        if (_allKeywords.Length > 0)
                        {
                            ShadersLimiter.Log($"Compiling {_shader.name} with keywords [{_allKeywords.GetKeywordsAsString(_shader)}]. Pass: {_passName}.", true);
                        }
                        else
                        {
                            ShadersLimiter.Log($"Compiling {_shader.name} with no keywords. Pass: {_passName}.", true);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Return true if shader variant should be stripped
        /// </summary>
        /// <param name="_currentShader">Shader to Strip reference</param>
        /// <param name="_snippet">Shader snippet</param>
        /// <param name="_allKeywords">All keywords of currently compiled shader</param>
        /// <returns></returns>
        private static bool stripShadersBasedOnStrippedKeywords(ShaderToStrip _currentShader, ShaderSnippetData _snippet, ShaderKeyword[] _allKeywords)
        {
            foreach (var _keyword in _currentShader.AvailableKeywords)
            {
                if (_keyword.StrippedGloballyOrLocally == false)
                {
                    continue;
                }

                if (_allKeywords.Contains(_keyword, _currentShader.ShaderReference))
                {
                    bool _printLogs = PrintStrippingLogs || _currentShader.ForcePrintLogsOnStrip;

                    if (_printLogs)
                    {
                        string _passName = _snippet.passType.ToString().InsertSpaceBeforeUpperCaseAndNumeric();

                        if (_allKeywords.Length > 0)
                        {
                            ShadersLimiter.Log($"Stripping shader {_currentShader.ShaderReference.name} variant with keywords [{_allKeywords.GetKeywordsAsString(_currentShader.ShaderReference)}]. Pass: {_passName}", true);
                        }
                        else
                        {
                            ShadersLimiter.Log($"Stripping shader {_currentShader.ShaderReference.name} variant with no keywords. Pass: {_passName}", true);
                        }
                    }

                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Return true if shader variant should be stripped. If shader variant is not in the database or it's not using conditional compilation, it won't be stripped.
        /// </summary>
        /// <param name="_currentShader">Shader to Strip reference</param>
        /// <param name="_snippet">Shader snippet</param>
        /// <param name="_allKeywords">All keywords of currently compiled shader</param>
        /// <returns></returns>
        private static bool stripShadersBasedOnShadersAllowedToCompile(ShaderToStrip _currentShader, ShaderSnippetData _snippet, ShaderKeyword[] _allKeywords)
        {
            if (_currentShader.AvailableVariants.IsNullOrEmpty())
            {
                return false;
            }

            CompiledShaderVariant _currentVariant = new CompiledShaderVariant(_currentShader.ShaderReference, _snippet, _allKeywords);

            if (_currentShader.IsAnyShaderVariantAMatch(_currentVariant))
            {
                return false;
            }

            bool _printLogs = PrintStrippingLogs || _currentShader.ForcePrintLogsOnStrip;

            if (_printLogs)
            {
                string _passName = _snippet.passType.ToString().InsertSpaceBeforeUpperCaseAndNumeric();

                if (_allKeywords.Length > 0)
                {
                    ShadersLimiter.Log($"Stripping shader {_currentShader.ShaderReference.name} variant with keywords [{_allKeywords.GetKeywordsAsString(_currentShader.ShaderReference)}]. Pass: {_passName}", true);
                }
                else
                {
                    ShadersLimiter.Log($"Stripping shader {_currentShader.ShaderReference.name} variant with no keywords. Pass: {_passName}", true);
                }
            }

            return true;
        }
    }
}
