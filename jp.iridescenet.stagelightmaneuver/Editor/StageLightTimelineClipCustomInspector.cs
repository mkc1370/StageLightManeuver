using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace StageLightManeuver.StageLightTimeline.Editor
{
    public abstract class EditorGUIWidth : System.IDisposable
    {
        protected abstract void ApplyWidth(float width);
        public EditorGUIWidth(float width) { ApplyWidth(width); }
        public void Dispose() { ApplyWidth(0.0f); }
    }

    public class LabelWidth : EditorGUIWidth
    {
        public LabelWidth(float width) : base(width) { }
        protected override void ApplyWidth(float width) { EditorGUIUtility.labelWidth = width; }
    }
    
    
    [CustomEditor(typeof(StageLightTimelineClip))]
    [CanEditMultipleObjects]
    public class StageLightTimelineClipCustomInspector : UnityEditor.Editor
    {

        
        private List<StageLightProfile> allProfilesInProject = new List<StageLightProfile>();
        private List<string> profileNames = new List<string>();
        private int selectedProfileIndex = 0;
        private static List<StageLightTimelineClip> selectedClips = new List<StageLightTimelineClip>();
        // group by folder
        private Dictionary<string, List<StageLightProfile>> folderNamesProfileDict = new Dictionary<string, List<StageLightProfile>>();
        

        private List<string> mExcluded = new List<string>();

        public override void OnInspectorGUI()
        {
            BeginInspector();
            mExcluded.Clear();
            
        }
        
        private void BeginInspector()
        {
            serializedObject.Update();

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("Profile", GUILayout.MaxWidth(60));
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(serializedObject.FindProperty("referenceStageLightProfile"),
                    new GUIContent(""));
                if (EditorGUI.EndChangeCheck())
                {
                    serializedObject.ApplyModifiedProperties();
                }
            }

            var stageLightTimelineClip = serializedObject.targetObject as StageLightTimelineClip;

            
            EditorGUI.BeginDisabledGroup(stageLightTimelineClip.referenceStageLightProfile == null);


            if(stageLightTimelineClip == null)
                return;
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                GUI.backgroundColor= Color.green;
                GUI.contentColor = Color.white;
                if (GUILayout.Button("Load Profile",GUILayout.MaxWidth(100)))
                {
                    // set dirty
                    EditorUtility.SetDirty(stageLightTimelineClip);
                    stageLightTimelineClip.LoadProfile();
                    serializedObject.ApplyModifiedProperties();
                    Repaint();
                }
                GUI.backgroundColor= Color.white;
                GUI.contentColor = Color.white;
                if (GUILayout.Button("Save Profile",GUILayout.MaxWidth(100)))
                {
                    stageLightTimelineClip.SaveProfile();
                    
                }
            }
            
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("syncReferenceProfile"));
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                stageLightTimelineClip.InitSyncData();
            }
            
            EditorGUI.EndDisabledGroup();


            using (new EditorGUILayout.HorizontalScope())
            {

                EditorGUI.BeginChangeCheck();
                var path = EditorGUILayout.PropertyField(serializedObject.FindProperty("exportPath"));
                if (EditorGUI.EndChangeCheck())
                {
                    serializedObject.ApplyModifiedProperties();
                }
                GUI.backgroundColor = Color.white;
                if (GUILayout.Button("...",GUILayout.MaxWidth(30)))
                {
                    SetFilePath(stageLightTimelineClip);
                }

            }

            using (new EditorGUILayout.HorizontalScope())
            {

                GUILayout.FlexibleSpace();



                GUI.backgroundColor= Color.red;
                GUI.contentColor = Color.white;
                
                if (GUILayout.Button("Save as",GUILayout.MaxWidth(100)))
                {
                    ExportProfile(stageLightTimelineClip);
                }
                
                GUI.backgroundColor = Color.white;
            }
            
            
            
            EditorGUILayout.Space(1);

           
            if (GUILayout.Button("Select StageLight",GUILayout.MaxWidth(120)))
            {
                if (stageLightTimelineClip.mixer != null && stageLightTimelineClip.mixer.trackBinding != null)
                {
                    var gameObjects = new List<GameObject>();
                    foreach (var stageLight in stageLightTimelineClip.mixer.trackBinding.AllStageLights)
                    {
                        gameObjects.Add(stageLight.gameObject);
                    }
                    Selection.objects = gameObjects.ToArray();
                }
                    
            }

            Selection.selectionChanged += () =>
            {
                selectedClips = SlmEditorUtility.SelectClips();
                SlmEditorUtility.InitAndProperties(stageLightTimelineClip.track.ReferenceStageLightProfile,selectedClips);  
                // Repaint();
            };
       
         
            var isMultiSelect = selectedClips.Count > 1;

            if (isMultiSelect)
            {

                if(selectedClips.Last() != stageLightTimelineClip)
                    return;
                
                var referenceProfile = stageLightTimelineClip.track.ReferenceStageLightProfile;
                var serializedProfile = new SerializedObject(referenceProfile);
                var stageLightPropertiesProperty = serializedProfile.FindProperty("stageLightProperties");
               
                if(stageLightPropertiesProperty == null)
                    return;
                
                for (int i = 0; i < referenceProfile.stageLightProperties.Count; i++)
                {   
                    
                    var property = referenceProfile.stageLightProperties[i];
                    if (property == null)
                    {
                        continue;
                    }
                    // Debug.Log(property.propertyName);
                    var serializedProperty = stageLightPropertiesProperty.GetArrayElementAtIndex(i);
                    if(serializedProperty == null)
                        continue;
                    StageLightProfileEditorUtil.DrawStageLightProperty(referenceProfile.stageLightProperties,serializedProperty ,false);

                    GUILayout.Space(2);
                    using (new EditorGUILayout.HorizontalScope())
                    {

                        GUILayout.FlexibleSpace();
                        if (GUILayout.Button("☑ Apply checked properties", GUILayout.Width(200)))
                        {
                            SlmEditorUtility.OverwriteProperties( referenceProfile, selectedClips);
                        }
                        GUILayout.FlexibleSpace();
                    }
                    GUILayout.Space(2);
            
                }
            }
            else
            {
                EditorGUI.BeginDisabledGroup(stageLightTimelineClip.syncReferenceProfile);
                var stageLightProperties = stageLightTimelineClip.behaviour.stageLightQueData.stageLightProperties;
                var behaviourProperty = serializedObject.FindProperty("behaviour");
                var stageLightQueDataProperty = behaviourProperty.FindPropertyRelative("stageLightQueData");
                var serializedProperty =stageLightQueDataProperty.FindPropertyRelative("stageLightProperties");
            
                for (int i = 0; i < stageLightProperties.Count; i++)
                {
                    if (stageLightProperties[i] == null)
                    {
                        stageLightProperties.RemoveAt(i);
                    }

                    serializedObject.ApplyModifiedProperties();
                }
                
                for (int i = 0; i < stageLightProperties.Count; i++)
                {

                    var property = stageLightProperties[i];
                    if(property == null) continue;
                
                    var serializedSlmProperty = serializedProperty.GetArrayElementAtIndex(i);
                    var expanded = false;
                    expanded = StageLightProfileEditorUtil.DrawHeader(serializedSlmProperty, property.propertyName);
                    
                    
                    if (!expanded)
                    {
                        continue;
                    }
                    EditorGUI.BeginDisabledGroup(!property.propertyOverride);
                    
                    // get serializable property name in property
                    var fields = property.GetType().GetFields().ToList();
                    var bpmOverride = fields.Find(x => x.Name == "bpmOverride");
                    if (bpmOverride != null)
                    {
                        fields.Remove(bpmOverride);
                        fields.Insert(0,bpmOverride);
                    }
                    fields.ForEach(f =>
                    {
                        StageLightProfileEditorUtil.DrawSlmToggleValue(serializedSlmProperty.FindPropertyRelative(f.Name));
                    });
                    var action = new Action(() =>
                    {
                        stageLightProperties.Remove(property);
                        return;
                    });
                    StageLightProfileEditorUtil.DrawRemoveButton(serializedObject,stageLightProperties, action);
                    
                    EditorGUI.EndDisabledGroup();
                }
            
                DrawAddPropertyButton(stageLightTimelineClip);
           
                EditorGUI.EndDisabledGroup();    
            }
            
        }
        
        private void DrawAddPropertyButton(StageLightTimelineClip stageLightTimelineClip)
        {
            EditorGUI.BeginChangeCheck();
            var selectList = new List<string>();
            
            SlmUtility.SlmPropertyTypes.ForEach(t =>
            {
                selectList.Add(t.Name);
            });
            
            selectList.Insert(0,"Add Property");
            foreach (var property in stageLightTimelineClip.behaviour.stageLightQueData
                         .stageLightProperties)
            {
               if(property == null) continue;
                if (selectList.Find(x => x== property.GetType().Name) != null)
                {
                    selectList.Remove(property.GetType().Name);
                }
                    
                
            }
            
            EditorGUI.BeginDisabledGroup(selectList.Count  <= 1);
            var select = EditorGUILayout.Popup(0, selectList.ToArray());
            EditorGUI.EndDisabledGroup();
            if (EditorGUI.EndChangeCheck())
            {
                // SetDirty 
                Undo.RecordObject(stageLightTimelineClip, "Add Property");
                EditorUtility.SetDirty(stageLightTimelineClip);   
                var type = SlmUtility.GetTypeByClassName(selectList[select]);
                var property = Activator.CreateInstance(type) as SlmProperty;

                if (property.GetType() == typeof(ManualLightArrayProperty))
                {
                    var manualLightArrayProperty = property as ManualLightArrayProperty;
                    var lightProperty = stageLightTimelineClip.behaviour.stageLightQueData.TryGet<LightProperty>();
                    var lightIntensityProperty = stageLightTimelineClip.behaviour.stageLightQueData.TryGet<LightIntensityProperty>();
                    if(lightProperty != null)
                    {
                        manualLightArrayProperty.initialValue.angle = lightProperty.spotAngle.value.constant;
                        manualLightArrayProperty.initialValue.innerAngle= lightProperty.innerSpotAngle.value.constant;
                        manualLightArrayProperty.initialValue.range = lightProperty.range.value.constant;
                    }
                    if(lightIntensityProperty != null)
                    {
                        manualLightArrayProperty.initialValue.intensity = lightIntensityProperty.lightToggleIntensity.value.constant;
                    }
                }
                stageLightTimelineClip.behaviour.stageLightQueData.stageLightProperties.Add(property);
                
                // apply serialized object
                serializedObject.ApplyModifiedProperties();
                //Save asset
                AssetDatabase.SaveAssets();
            }
            
            
        }

        private void SetFilePath(StageLightTimelineClip stageLightTimelineClip)
        {
            var exportPath = stageLightTimelineClip.referenceStageLightProfile != null ? AssetDatabase.GetAssetPath(stageLightTimelineClip.referenceStageLightProfile) : "Asset";
            var exportName = stageLightTimelineClip.referenceStageLightProfile != null ? stageLightTimelineClip.referenceStageLightProfile.name+"(Clone)" : "new stageLightProfile";
            var path = EditorUtility.SaveFilePanel("Save StageLightProfile Asset", exportPath,exportName, "asset");
            string fileName = Path.GetFileName(path);
            if(path == "") return;
            path = path.Replace("\\", "/").Replace(Application.dataPath, "Assets");
            string dir = Path.GetDirectoryName(path);
            stageLightTimelineClip.exportPath = path;

            serializedObject.ApplyModifiedProperties();
        }


        private void ExportProfile(StageLightTimelineClip stageLightTimelineClip)
        {
           
            Undo.RegisterCompleteObjectUndo(stageLightTimelineClip, stageLightTimelineClip.name);
            EditorUtility.SetDirty(stageLightTimelineClip);
            
            var newProfile = CreateInstance<StageLightProfile>();
            newProfile.stageLightProperties = stageLightTimelineClip.behaviour.stageLightQueData.stageLightProperties;
            var exportPath = SlmUtility.GetExportPath(stageLightTimelineClip.exportPath,stageLightTimelineClip.clipDisplayName) + ".asset";

            // if directory not exist, create it
            var directory = Path.GetDirectoryName(exportPath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            
            
            // if same name file exist in directory, add (number) to file name
            var fileName = Path.GetFileNameWithoutExtension(exportPath);
            var fileExtension = Path.GetExtension(exportPath);
            var filePath = Path.GetDirectoryName(exportPath);
            
            // try .asset file 
            var files = Directory.GetFiles(filePath, "*" + fileExtension).ToList().Where( f => f.Contains(fileName)).ToList();
            var fileNames = files.Select(f => Path.GetFileNameWithoutExtension(f)).ToList();
            // sort file names
            fileNames.Sort();
            
            // fileNames.ForEach(f => Debug.Log(f));
            var lastFileNumber = 0;
            var exportFileName = fileName;
            if (fileNames.Count > 0)
            {
                var lastFile = fileNames.Last();
                var match = Regex.Match(lastFile, @"\((\d+)\)$");
                if (match.Success)
                {
                    lastFileNumber = int.TryParse (match.Groups[1].Value, out lastFileNumber) ? lastFileNumber : 0;
                }

                fileName = fileName.Replace($"({lastFileNumber})", "");
                lastFileNumber++;
            }

            if (lastFileNumber == 0)
            {
                exportPath = filePath + "/" + fileName + fileExtension;
            }
            else
            {
                exportPath = filePath + "/" + fileName+ $"({lastFileNumber})" + fileExtension;
            }
            
                




            AssetDatabase.CreateAsset(newProfile, exportPath);
            AssetDatabase.Refresh();
            InitProfileList(stageLightTimelineClip);
            stageLightTimelineClip.referenceStageLightProfile = AssetDatabase.LoadAssetAtPath<StageLightProfile>(exportPath);
            // EditorUtility.SetDirty(stageLightTimelineClip);
            AssetDatabase.SaveAssets();
            // serializedObject.Applyy(stageLightTimelineClip);
            // AssetDatabase.SaveAssets();
            // serializedObject.ApplyModifiedProperties();
            //
        }
        
        
        private void OnDisable()
        {
           
        }

        private void OnDestroy()
        {
        }
        public void OnInspectorUpdate()
        {
            this.Repaint();
        }
        
        private void InitProfileList(StageLightTimelineClip stageLightTimelineClip)
        {
            allProfilesInProject = SlmUtility.GetProfileInProject();
            profileNames.Clear();

            // group by folder
            folderNamesProfileDict = new Dictionary<string, List<StageLightProfile>>();
            foreach (var profile in allProfilesInProject)
            {
                var path = AssetDatabase.GetAssetPath(profile);
                var parentDirectory = Path.GetDirectoryName(path).Replace("Assets/", "").Replace("Assets\\", "");
                parentDirectory = parentDirectory.Replace("\\", ">").Replace("/", ">");
                if (folderNamesProfileDict.ContainsKey(parentDirectory))
                {
                    folderNamesProfileDict[parentDirectory].Add(profile);
                }
                else
                {
                    folderNamesProfileDict.Add(parentDirectory, new List<StageLightProfile> {profile});
                }

            }

            foreach (var keyPair in folderNamesProfileDict)
            {
                foreach (var v in keyPair.Value)
                {
                    profileNames.Add($"{keyPair.Key}/{v.name}");
                }
            }
            
            selectedProfileIndex = allProfilesInProject.IndexOf(stageLightTimelineClip.referenceStageLightProfile);
        }
        
        private void DrawProfilesPopup(StageLightTimelineClip stageLightTimelineClip)
        {
            
            if(allProfilesInProject == null || allProfilesInProject.Count == 0)
                InitProfileList(stageLightTimelineClip);
            
            EditorGUI.BeginChangeCheck();
            selectedProfileIndex = EditorGUILayout.Popup("", selectedProfileIndex, profileNames.ToArray(), GUILayout.Width(120));
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(stageLightTimelineClip, "Changed StageLightProfile");
                stageLightTimelineClip.referenceStageLightProfile = allProfilesInProject[selectedProfileIndex];
                serializedObject.ApplyModifiedProperties();
            }
        }

        
        


    }
}