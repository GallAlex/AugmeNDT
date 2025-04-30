# AugmeNDT

<div align="center">

**Augmented Reality for Non-Destructive Testing (NDT)**

*Advanced immersive visualization and analysis of volumetric datasets*

[![Unity](https://img.shields.io/badge/Unity-6000.0.37f1-blue.svg)](https://unity.com/)
[![MRTK3](https://img.shields.io/badge/MRTK-3.0-brightgreen.svg)](https://github.com/MixedRealityToolkit/MixedRealityToolkit-Unity)
[![Math.NET](https://img.shields.io/badge/Math.NET-5.0.0-orange.svg)](https://numerics.mathdotnet.com/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

</div>

#### ![AugmeNDT](https://github.com/user-attachments/assets/6db0d7d2-e6a0-4a48-b686-b6d8ba011409)

## Overview

AugmeNDT is a Unity-based framework for immersive analysis of volumetric datasets with a focus on industrial computed tomography (CT) data. It enables researchers and analysts to visualize and interact with complex material data in augmented reality, providing more intuitive and efficient workflows for material inspection and analysis.

Currently compatible with Microsoft HoloLens 2 and Magic Leap 2 devices.

### Dependencies

| Name                                                                    | Version     |
| ----------------------------------------------------------------------- | ----------- |
| Unity                                                                   | 6000.0.37f1 |
| C#                                                                      | 9.0         |
| .NET Standard                                                           | 2.1         |
| .NET Framework                                                          | 4.8         |
| [MRTK3](https://github.com/MixedRealityToolkit/MixedRealityToolkit-Unity/releases) | 3.0 |
| [Math.NET Numerics](https://numerics.mathdotnet.com/#Math-NET-Numerics) | 5.0.0       |

For easy loading of NuGet packages in Unity, we recommend:

| Name                                                                      | Version  |
| ------------------------------------------------------------------------- | -------- |
| [NuGetForUnity](https://github.com/GlitchEnzo/NuGetForUnity/releases)     | 3.0.5    |

## Featured Applications

AugmeNDT serves as the foundation for two powerful research applications:

### 1. MARV: Multiview Augmented Reality Visualisation

**MARV** [[1]](#marv) enables the exploration of rich material data through novel immersive visual analytics techniques. It makes the analysis of complex, large, and heterogeneous material data more effective and engaging in an augmented reality setting.

#### Key Features:
- **Multidimensional Distribution Glyphs (MDD Glyphs) with Skewness Kurtosis Mapper**: Provide a visual summary of statistical characteristics of the attributes of interest.
- **Temporal Evolution Tracker**: Visualises changes in the distributions of these attributes over time.
- **Chrono Bins**: Interactive exploration of multidimensional distributions across multiple time steps to compare the severity of changes between datasets.


### 2. Situated Analytics for Material Inspection

This prototype [[2]](#immersive-inspection) enables **on-site, in-place inspection** of material properties by overlaying X-ray computed tomography (XCT) data directly onto physical objects in augmented reality.

> **Note:** The Immersive Inspection tool is available in the [`SituatedAnalysis`]([[https://github.com/yourusername/AugmeNDT/tree/immersive-inspection](https://github.com/GallAlex/AugmeNDT/tree/SituatedAnalysis)](https://github.com/GallAlex/AugmeNDT/tree/SituatedAnalysis)) branch, which requires Vuforia 10.19.3 in addition to the dependencies listed in that branch.

#### Key Features:
- Real-time alignment of digital XCT data with physical specimens
- Exploration of both primary and secondary XCT data
- Intuitive workflows for identifying anomalies in fiber-reinforced polymer materials


## Installation

1. Clone this repository:
   ```
   git clone https://github.com/yourusername/AugmeNDT.git
   ```
2. Choose the branch for your desired application:
   - For MARV (Unity 6): `git checkout main` or (Unity 2021) `git checkout MRTK2.8_Unity2021`
   - For Immersive Inspection: `git checkout SituatedAnalysis`
3. Open the project in the appropriate Unity version
4. Install the required dependencies for your chosen branch


## Citing This Work

If you use AugmeNDT or its applications in your research, please cite the following papers:

<a id="marv"></a>
### MARV

```bibtex
@Article{Gall2024a,
  author        = {Gall, Alexander and Heim, Anja and Gröller, Eduard and Heinzl, Christoph},
  journal       = {Computer Graphics Forum},
  title         = {MARV: Multiview Augmented Reality Visualisation for Exploring Rich Material Data},
  year          = {2025},
  doi           = {10.1111/cgf.70150},
  eprint        = {2404.14814},
  publisher     = {Wiley},
}
```

<a id="immersive-inspection"></a>
### Immersive Inspection

```bibtex
@Article{Gall2024,
  author        = {Gall, Alexander and Heim, Anja and Weinberger, Patrick and Fröhler, Bernhard and Kastner, Johann and Heinzl, Christoph},
  journal       = {{arXiv Preprints}},
  title         = {Immersive Analysis: Enhancing Material Inspection of X-Ray Computed Tomography Datasets in Augmented Reality},
  year          = {2024},
  month         = apr,
  archiveprefix = {arXiv},
  copyright     = {arXiv.org perpetual, non-exclusive license},
  creationdate  = {2024-05-01T00:00:00},
  doi           = {10.48550/ARXIV.2404.12751},
  eprint        = {2404.12751},
  keywords      = {FOS: Computer and information sciences, Graphics (cs.GR), Human-Computer Interaction (cs.HC)},
  primaryclass  = {cs.HC},
  publisher     = {arXiv},
}
```

## Contact

Developed and maintained by 
**Alexander Gall**  

Email: [alexander.gall@uni-passau.de](mailto:alexander.gall@uni-passau.de)  
LinkedIn: [LinkedIn Profile](https://www.linkedin.com/in/alexander-gall-1b7039242)  
Website: [Homepage](https://sites.google.com/view/alexandergall/)
