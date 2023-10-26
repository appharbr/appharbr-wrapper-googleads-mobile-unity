// Copyright (C) 2023 Google LLC
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

#if UNITY_ANDROID
using UnityEditor.Android;
using UnityEngine;
using System.IO;
using System;

namespace GoogleMobileAds.Editor
{
    /// <summary>
    /// Fixes kotlinx_coroutines_core.version collision errors.
    /// For more details see https://developers.google.com/admob/unity/gradle.
    /// </summary>
    class FixKotlinBuildProcessor : IPostGenerateGradleAndroidProject
    {
        public int callbackOrder { get { return 0; } }

        private string includeInstruction;

        private GoogleMobileAdsSettings settings;

        private const string excludeInstruction = "packagingOptions {";

        public FixKotlinBuildProcessor()
        {
            includeInstruction = "packagingOptions {" +
                Environment.NewLine +
                "\t\tpickFirst \"META-INF/kotlinx_coroutines_core.version\"";
            settings = GoogleMobileAdsSettings.LoadInstance();
        }

        public void OnPostGenerateGradleAndroidProject(string path)
        {
            // Get the root android project file path.
            var rootDirinfo = new DirectoryInfo(path);
            var rootPath = rootDirinfo.Parent.FullName;

            // Get the launcher build.gradle
            var gradles = Directory.GetFiles(rootPath,
                "launcher/build.gradle",
                SearchOption.AllDirectories);

            foreach (var gradlePath in gradles)
            {
                // Find the file with packagingOptions to edit.
                var gradle = File.ReadAllText(gradlePath);
                var hasOption = gradle.Contains("packagingOptions");
                if (!hasOption)
                    continue;

                // Check if we have added out instruction
                var hasInstruction = gradle.Contains(includeInstruction);

                if (settings.EnableFixKotlinBuildProcessor)
                {
                    // Add the instruction if not included.
                    if (!hasInstruction)
                    {
                        // Add the pickfirst instruction.
                        var newInstruction = gradle.Replace(
                            excludeInstruction,
                            includeInstruction);

                        // Override the updated gradle.build
                        File.WriteAllText(gradlePath, newInstruction);
                        Debug.LogFormat("Google Mobile Ads FixKotlinBuildProcessor fix has been added to {0}.",
                            gradlePath);
                    }
                }
                else
                {
                    // Remove the instruction if we added it.
                    if (hasInstruction)
                    {
                        var newInstruction = gradle.Replace(
                            includeInstruction,
                            excludeInstruction);

                        // Override the updated gradle.build
                        File.WriteAllText(gradlePath, newInstruction);
                        Debug.LogFormat("Google Mobile Ads FixKotlinBuildProcessor fix has been removed from {0}.",
                            gradlePath);
                    }
                }
            }
        }
    }
}
#endif