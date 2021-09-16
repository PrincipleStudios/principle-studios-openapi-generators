import { join } from "path";
import { readFileSync, writeFileSync, readdirSync, statSync } from "fs";
import { CLIEngine } from "eslint";

const getAllFiles = function(dirPath: string, arrayOfFiles: string[] = []) {
  const files = readdirSync(dirPath);

  files.forEach(function(file) {
    const fullFile = join(dirPath, file);
    if (statSync(fullFile).isDirectory()) {
      arrayOfFiles = getAllFiles(fullFile, arrayOfFiles);
    } else {
      arrayOfFiles.push(fullFile);
    }
  });

  return arrayOfFiles;
};

export function verifyAllFiles(outputDir: string) {
  it("can generate the files", () => {
    const files = getAllFiles(outputDir);

    files.forEach(file => {
      if (file.endsWith(".ts")) {
        const contents = readFileSync(file).toString();
        expect(contents).toMatchSnapshot(file);
      }
    });
  });
}
