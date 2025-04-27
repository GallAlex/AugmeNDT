# AugmeNDT

A Unity application for the immersive analysis of volumetric datasets, with a focus on industrial computed tomography datasets.
Currently tested with the Microsoft Hololens 2.

Dependencies and versions utilized:
| Name                                                                    | Version  |
| ----------------------------------------------------------------------- | -------- |
| Unity                                                                   | 2021.3.30f1 |
| C#                                                                      | 9.0      |
| .NET Standard                                                  | 2.1      |
| .NET Framework                                             | 4.8      |
| [MRTK](https://github.com/microsoft/MixedRealityToolkit-Unity/releases)                                                                   | 2.8.3.0  |
| [Math.NET Numerics](https://numerics.mathdotnet.com/#Math-NET-Numerics)| 5.0.0    | 

For easy loading of NuGet packages in Unity, the NuGetForUnity Asset can be used. 
| Name                                                                      | Version  |
| ------------------------------------------------------------------------- | -------- |
| **[NuGetForUnity](https://github.com/GlitchEnzo/NuGetForUnity/releases)** | 3.0.5 |

## Topological Data Analysis

AugmeNDT provides a seamless Topological Data Analysis experience for segmented data in immersive systems.
After analyzing and preparing .mhd and .raw files using libraries such as TTK or VTK, you can explore 2D and 3D visualizations of streamlines, glyphs, and flow fields in AugmeNDT. Alternatively, you can directly use the built-in TTK and VTK integrations within AugmeNDT.

For more information, please refer to the **TopologicalDataAnalysis_UserGuide.pdf** file located in the **Documents** folder.

## HTC Vive Pro 2 Usage

Install the latest version of the following software on your computer. 

> **Note:**
> If you are using SteamVR or your HTC Vive Pro 2 device for the first time, make sure to set up the "Room" configuration through SteamVR. Otherwise, you may not have an optimal VR experience.

 
| Name                                                                      | Version  |
| ------------------------------------------------------------------------- | -------- |
| **[SteamVR](https://store.steampowered.com/app/250820/SteamVR/)** | latest version |
| **[VIVE Console (VIVE Console for SteamVR) ](https://store.steampowered.com/app/250820/SteamVR/)** | latest version |

> **Note For Developers:**
> In the **Edit > Project Settings > XR Plug-in Management** section,**"Initialize XR on Startup"** and **"OpenXR"** must be enabled. BUT, both the **"Holographic Remoting remote app feature group"** and the **"Windows Mixed Reality feature group"** options must be **disabled**. You can safely ignore the error shown in the Project Validation section.

> **Second Note For Developers:**
> For devices like the HTC Vive Pro 2 and other SteamVR-supported headsets, no additional setup is required. This setup can be used with any system that is compatible with SteamVR.
> For systems that are not integrated with SteamVR, it is sufficient to use the device's own plugin along with the SteamVR plugin.
> The Windows Mixed Reality feature group shares some files with the Holographic Remoting remote app feature group. As a result, it tries to launch XR through the Holographic Remoting remote app, which causes a conflict. Therefore, both of these options should be disabled.

---
Developed and maintained by Alexander Gall

Contact: [alexander.gall@fh-wels.at](alexander.gall@fh-wels.at)
