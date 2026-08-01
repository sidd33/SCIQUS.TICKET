# Sciqus Mobile Frontend Setup Guide

This guide contains all the configuration and steps required to run this React Native application on a new system (Windows, macOS, or Linux).

## 1. Prerequisites

Before starting, ensure you have the following installed on your system:

### Node.js & npm
- Download and install **Node.js** (v18 or newer is recommended): [Node.js Official Site](https://nodejs.org/)
- Verify installation by running: `node -v` and `npm -v`

### Java Development Kit (JDK)
- React Native requires **Java 17**.
- You can install it via Microsoft Build of OpenJDK or Azul Zulu.
- **Windows (Chocolatey):** `choco install openjdk17`
- **macOS (Homebrew):** `brew install openjdk@17`

### Exact Versions Used in this Project
For guaranteed compatibility, the following exact versions are configured and tested:
- **Node.js**: v18.x or higher
- **React Native**: 0.74.5
- **Java (JDK)**: 17
- **Gradle**: 8.6
- **Kotlin**: 1.9.22
- **Android Target SDK**: 34
- **Android Min SDK**: 24
- **Android Build Tools**: 34.0.0

### Android Studio (For Android Development)
1. Download and install [Android Studio](https://developer.android.com/studio).
2. During installation, make sure the following are checked:
   - `Android SDK`
   - `Android SDK Platform`
   - `Android Virtual Device (AVD)`
3. Open Android Studio -> SDK Manager (Tools > SDK Manager) and ensure **Android 14 (API Level 34)** is installed.
4. Under the "SDK Tools" tab, click "Show Package Details" at the bottom right, and ensure **Android SDK Build-Tools 34.0.0** is checked and installed.

---

## 2. Environment Variables Configuration

You MUST configure your environment variables for React Native to find the Android SDK.

### Windows
1. Open Windows Search and type "Environment Variables" -> click **Edit the system environment variables**.
2. Click **Environment Variables...**
3. Under "User variables", click **New...**
   - **Variable name:** `ANDROID_HOME`
   - **Variable value:** `C:\Users\YOUR_USERNAME\AppData\Local\Android\Sdk` *(Replace YOUR_USERNAME with your actual Windows username)*
4. Next, select the `Path` variable under "User variables" and click **Edit...**
5. Add this new entry: `%ANDROID_HOME%\platform-tools`
6. Click OK and restart your terminal/command prompt.

### macOS / Linux
Open your shell configuration file (e.g., `~/.zshrc` or `~/.bash_profile`) and add:
```bash
export ANDROID_HOME=$HOME/Library/Android/sdk
export PATH=$PATH:$ANDROID_HOME/emulator
export PATH=$PATH:$ANDROID_HOME/platform-tools
```
Run `source ~/.zshrc` to apply the changes.

---

## 3. Project Installation

Once the prerequisites are installed, follow these steps to set up the project:

1. Clone the repository and navigate to the mobile frontend folder:
   ```bash
   cd Mobile-Frontend
   ```

2. Install the project dependencies:
   ```bash
   npm install
   ```

---

## 4. Running the Application

### Start the Metro Bundler
The Metro bundler is the JavaScript server for React Native. Keep this running in its own terminal window.
```bash
npm start
```

### Run on Android Emulator
In a separate terminal window, ensure your Android Virtual Device (AVD) is running, or let React Native launch it automatically:
```bash
npm run android
```

### Run on iOS (macOS only)
If you are on a Mac and want to run the app on an iPhone simulator, you must install CocoaPods first:
```bash
cd ios
pod install
cd ..
npm run ios
```

---

## Troubleshooting

- **"Command not found: react-native" or SDK errors:** Double-check your `ANDROID_HOME` environment variable and ensure `platform-tools` is in your `PATH`.
- **EPERM / EBUSY errors on Windows:** Close Android Studio, stop the Metro bundler, and run `npm start -- --reset-cache`.
- **Backend Connection Issues:** If the mobile app fails to log in, ensure your `.NET` backend is running on `localhost`. By default, the Android emulator maps `10.0.2.2` to your computer's localhost. If your API is running on a different port, update the `API_BASE_URL` in `src/services/api.ts`.
