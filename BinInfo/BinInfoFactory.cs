using System;
using System.IO;

using BinInfo.ELF;
using BinInfo.MachO;
using BinInfo.Utils;

namespace BinInfo {
    public static class BinInfoFactory {
        public static IBinInfo GetBinInfo(string filePath) {
            var fileData = File.ReadAllBytes(filePath);
            
            if (fileData.Length > 4 && fileData[0] == 0x7F && fileData[1] == 0x45 && fileData[2] == 0x4C && fileData[3] == 0x46 && fileData[0x12] == 0x28) { // aarch32
                string toolchainFolder;
                string toolPrefix = "arm-eabi-";

                // TODO: What should be the order? Should the Path or the bundled toolchain have priority?
                // TODO: Make toolchain folder platform specific (./toolchains/win/...?)
                if (PathUtils.ExistsOnPath("arm-eabi-nm")) {
                    toolchainFolder = Path.Combine(PathUtils.GetFullPath("arm-eabi-nm"), "..");
                }
                else if (PathUtils.ExistsOnPath("arm-none-eabi-nm")) {
                    toolchainFolder = Path.Combine(PathUtils.GetFullPath("arm-none-eabi-nm"), "..");
                    toolPrefix = "arm-none-eabi-";
                }
                else if (Directory.Exists("toolchains")) {
                    toolchainFolder = "toolchains/arm-eabi/bin";
                }
                else if (Directory.Exists("../../../toolchains/arm-eabi/bin")) {
                    toolchainFolder = "../../../toolchains/arm-eabi/bin";
                }
                else {
                    throw new Exception("Failed to find toolchain!");
                }

                toolchainFolder = Path.GetFullPath(toolchainFolder);
                
                return new ELFBinInfo(ELFType.Aarch32, filePath, toolchainFolder, toolPrefix);
            }
            else if (fileData.Length > 4 && fileData[0] == 0x7F && fileData[1] == 0x45 && fileData[2] == 0x4C && fileData[3] == 0x46 && fileData[0x12] == 0xb7) { // aarch64
                string toolchainFolder;
                
                if (PathUtils.ExistsOnPath("aarch64-elf-nm")) {
                    toolchainFolder = PathUtils.GetFullPath("aarch64-elf-nm");
                }
                else if (Directory.Exists("toolchains")) {
                    toolchainFolder = "toolchains/aarch64/bin";
                }
                else {
                    toolchainFolder = "../../../toolchains/aarch64/bin";
                }
                
                toolchainFolder = Path.GetFullPath(toolchainFolder);
                
                return new ELFBinInfo(ELFType.Aarch64, filePath, toolchainFolder, "aarch64-elf-");
            }
            else {
                return new MachOBinInfo(filePath);
            }
        }
    }
}