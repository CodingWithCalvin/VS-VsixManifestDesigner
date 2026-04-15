<p align="center">
  <img src="https://raw.githubusercontent.com/CodingWithCalvin/VS-VsixManifestDesigner/main/resources/logo.png" alt="VSIX Manifest Designer Logo" width="128" height="128">
</p>

<h1 align="center">VSIX Manifest Designer</h1>

<p align="center">
  <strong>🎨 A modern visual designer for VSIX manifest files in Visual Studio 2022!</strong>
</p>

<p align="center">
  <a href="https://github.com/CodingWithCalvin/VS-VsixManifestDesigner/blob/main/LICENSE">
    <img src="https://img.shields.io/github/license/CodingWithCalvin/VS-VsixManifestDesigner?style=for-the-badge" alt="License">
  </a>
  <a href="https://github.com/CodingWithCalvin/VS-VsixManifestDesigner/actions/workflows/build.yml">
    <img src="https://img.shields.io/github/actions/workflow/status/CodingWithCalvin/VS-VsixManifestDesigner/build.yml?style=for-the-badge" alt="Build Status">
  </a>
</p>

---

## 🤔 Why?

The built-in VSIX manifest designer in Visual Studio is old, outdated, and rather ugly. **VSIX Manifest Designer** is a modern replacement with a clean, intuitive UI that feels right at home in Visual Studio 2022!

## ✨ Features

- **📝 Metadata Editor** - Edit identity, display name, description, icons, and more
- **🎯 Installation Targets** - Configure supported VS SKUs and version ranges
- **🔗 Dependencies** - Manage dependencies on other extensions with ease
- **⚡ Prerequisites** - Define required VS components
- **📦 Assets** - Add and organize extension assets with type selection and project picker
- **📁 Content** - Configure project and item templates
- **🎨 VS Theme Support** - Seamlessly integrates with Light, Dark, and Blue themes

## 💡 Pro Tip: Use VsixSdk!

For the best VSIX development experience, pair this extension with [CodingWithCalvin.VsixSdk](https://github.com/CodingWithCalvin/VsixSdk)! VsixSdk modernizes the VSIX build process with SDK-style projects, and VSIX Manifest Designer fully supports it - including smart project detection and template asset handling.

## 🖼️ Screenshots

### Metadata
![Metadata Editor](https://raw.githubusercontent.com/CodingWithCalvin/VS-VsixManifestDesigner/main/resources/metadata.png)

### Installation Targets
![Installation Targets](https://raw.githubusercontent.com/CodingWithCalvin/VS-VsixManifestDesigner/main/resources/install-targets.png)

### Dependencies
![Dependencies](https://raw.githubusercontent.com/CodingWithCalvin/VS-VsixManifestDesigner/main/resources/dependencies.png)

### Prerequisites
![Prerequisites](https://raw.githubusercontent.com/CodingWithCalvin/VS-VsixManifestDesigner/main/resources/prerequisites.png)

### Assets
![Assets](https://raw.githubusercontent.com/CodingWithCalvin/VS-VsixManifestDesigner/main/resources/assets.png)

### Content
![Content](https://raw.githubusercontent.com/CodingWithCalvin/VS-VsixManifestDesigner/main/resources/content.png)

## 🛠️ Installation

### Visual Studio Marketplace

1. Open Visual Studio 2022
2. Go to **Extensions > Manage Extensions**
3. Search for "VSIX Manifest Designer"
4. Click **Download** and restart Visual Studio

### Manual Installation

Download the latest `.vsix` from the [Releases](https://github.com/CodingWithCalvin/VS-VsixManifestDesigner/releases) page and double-click to install.

## 🚀 Usage

1. Open any `source.extension.vsixmanifest` file in your solution
2. The visual designer opens automatically
3. Edit your manifest using the tabbed interface
4. Save and you're done! 🎉

## 🤝 Contributing

Contributions are welcome! Whether it's bug reports, feature requests, or pull requests - all feedback helps make this extension better.

### Development Setup

1. Clone the repository
2. Open `src/CodingWithCalvin.VsixManifestDesigner.slnx` in Visual Studio 2022
3. Ensure you have the "Visual Studio extension development" workload installed
4. Press F5 to launch the experimental instance

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

## 👥 Contributors

<!-- readme: contributors -start -->
<a href="https://github.com/CalvinAllen"><img src="https://avatars.githubusercontent.com/u/41448698?v=4&s=64" width="64" height="64" alt="CalvinAllen"></a> 
<!-- readme: contributors -end -->

---

<p align="center">
  Made with ❤️ by <a href="https://github.com/CodingWithCalvin">Coding With Calvin</a>
</p>
