using System;
using System.Collections.Generic;
using System.IO;
using Idem.Configuration;
using Idem.Tools;
using UnityEditor;
using UnityEngine;

namespace Idem.Editor
{
    [CustomEditor(typeof(IdemConfiguration))]
    public class IdemConfigurationEditor : UnityEditor.Editor
    {
        private const string CredentialsPrefsKey = "IdemConfigurationEditor_Credentials";
        private const string IdemWebsite = "https://docs.idem.gg";
        private const string IdemLogoTexturePath = "Idem/IdemLogo";
        private static readonly Dictionary<string, Texture2D> TextureCache = new();
        private readonly IdemCredentials _prevCredentials = new();
        private readonly HashSet<string> _knownInstances = new();
        private IdemConfiguration _castedTarget;
        private IdemCredentials _credentials;

        private bool CredentialsDirty => !_prevCredentials.Equals(_credentials);

        public override void OnInspectorGUI()
        {
            InitIfNeeded();

            DrawTextureButton(IdemLogoTexturePath, OpenIdemWebsite, padTop: 10f, padBottom: 10f, padLeft: 0.02f,
                padRight: 0.3f);
            
            if (_knownInstances.Count > 1)
            {
                EditorGUILayout.HelpBox(
                    "Multiple IdemConfiguration instances detected. This may lead to unexpected behavior. " +
                    "Please ensure there is only one IdemConfiguration instance in your project.",
                    MessageType.Warning);
            }

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.LabelField("Configuration", EditorStyles.boldLabel);
            _castedTarget.Config.gameId = EditorGUILayout.TextField("Game mode ID", _castedTarget.Config.gameId);
            _castedTarget.Config.quitOnCritError =
                EditorGUILayout.Toggle("Quit on critical error", _castedTarget.Config.quitOnCritError);
            _castedTarget.Config.quitAfterResultReporting = EditorGUILayout.Toggle("Quit after results reported",
                _castedTarget.Config.quitAfterResultReporting);
            _castedTarget.Config.autoRestartServerIdemConnect =
                EditorGUILayout.Toggle("Auto restart server Idem connect",
                    _castedTarget.Config.autoRestartServerIdemConnect);
            _castedTarget.Config.maxIdemConnectAttempts = EditorGUILayout.IntField("Max Idem connect attempts",
                _castedTarget.Config.maxIdemConnectAttempts);
            _castedTarget.Config.debugLogging =
                EditorGUILayout.Toggle("Verbose logging", _castedTarget.Config.debugLogging);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Server", EditorStyles.boldLabel);
            _castedTarget.Config.serverType =
                (EServerType)EditorGUILayout.EnumPopup("Idem server", _castedTarget.Config.serverType);
            if (_castedTarget.Config.serverType == EServerType.Custom)
            {
                _castedTarget.Config.customUrl =
                    EditorGUILayout.TextField("Server URL", _castedTarget.Config.customUrl);
                _castedTarget.Config.customClientId =
                    EditorGUILayout.TextField("Client ID", _castedTarget.Config.customClientId);
            }

            if (EditorGUI.EndChangeCheck()) EditorUtility.SetDirty(target);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Credentials", EditorStyles.boldLabel);

            _prevCredentials.CopyFrom(_credentials);
            _credentials.JoinCode = EditorGUILayout.TextField(nameof(IdemCredentials.JoinCode), _credentials.JoinCode);
            _credentials.UserName = EditorGUILayout.TextField(nameof(IdemCredentials.UserName), _credentials.UserName);
            _credentials.Password = EditorGUILayout.TextField(nameof(IdemCredentials.Password), _credentials.Password);

            if (CredentialsDirty) EditorPrefs.SetString(CredentialsPrefsKey, _credentials.ToJson());

            if (GUILayout.Button("Apply config"))
            {
                AssetDatabase.SaveAssetIfDirty(target);
                var configPath = AssetDatabase.GetAssetPath(target);
                var configDir = Path.GetDirectoryName(configPath);
                ConfigGenerator.Generate(configDir, _credentials.JoinCode, _credentials.UserName, _credentials.Password);
            }
        }

        private void InitIfNeeded()
        {
            if (_castedTarget != null)
                return;

            _castedTarget = target as IdemConfiguration;

            var previous = EditorPrefs.GetString(CredentialsPrefsKey);
            _credentials = IdemCredentials.FromJsonOrEmpty(previous);

            var allInstances = AssetDatabase.FindAssets("t:IdemConfiguration");
            Array.ForEach(allInstances, guid =>
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                _knownInstances.Add(path);
            });
        }

        private void OpenIdemWebsite()
        {
            Application.OpenURL(IdemWebsite);
        }

        private void DrawTextureButton(string resourcesPath, Action onClick, Color? bgColor = null,
            float padTop = 0f, float padBottom = 0f, float padLeft = 0f, float padRight = 0f)
        {
            if (!TextureCache.TryGetValue(resourcesPath, out var tex))
            {
                tex = Resources.Load<Texture2D>(resourcesPath);
                if (tex != null)
                    TextureCache[resourcesPath] = tex;
            }

            if (tex == null)
                return;

            var availableWidth = EditorGUIUtility.currentViewWidth;
            if (padLeft > 0f && padLeft < 1f)
                padLeft *= availableWidth;
            if (padRight > 0f && padRight < 1f)
                padRight *= availableWidth;
            if (padTop > 0f && padTop < 1f)
                padTop *= availableWidth;
            if (padBottom > 0f && padBottom < 1f)
                padBottom *= availableWidth;

            var scaledWidth = Mathf.Min(tex.width, availableWidth - padLeft - padRight);
            var scaledHeight = tex.height * (scaledWidth / tex.width);
            var totalHeight = scaledHeight + padTop + padBottom;

            if (bgColor.HasValue)
            {
                var totalRect = new Rect(0, 0, availableWidth, totalHeight);
                EditorGUI.DrawRect(totalRect, bgColor.Value);
            }

            var texRect = new Rect(
                padLeft,
                padTop,
                scaledWidth,
                scaledHeight
            );

            var content = new GUIContent(tex);
            if (GUI.Button(texRect, content, GUIStyle.none)) onClick?.Invoke();


            GUILayout.Space(totalHeight);
        }
    }
}