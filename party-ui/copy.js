// Import the required modules
const fs = require('fs');
const path = require('path');

// Get the directory path, destination directory, new file name from command line arguments
const [,, dirPath, destDir, newFileName, extension] = process.argv;

// Validate input
if (!dirPath || !destDir || !newFileName) {
    console.error('Usage: node ./copy.js <directory-path> <destination-directory> <new-file-name>');
    process.exit(1);
}

// Read the specified directory and find the first matching .js file
fs.readdir(dirPath, (err, files) => {
    if (err) {
        console.error(`Error reading directory: ${err.message}`);
        process.exit(1);
    }

    console.log(`Looking for files in directory: ${dirPath}`);
    console.log(`Available files: ${files.join(', ')}`);

    // Filter for .js files
    const matchingFiles = files.filter(file => file.endsWith(extension));

    // Check if any .js files were found
    if (matchingFiles.length === 0) {
        console.error(`No ${extension} files found in the directory: ${dirPath}`);
        process.exit(1);
    }

    // Use the first .js file found
    const source = path.resolve(dirPath, matchingFiles[0]);
    const destination = path.resolve(destDir, newFileName);

    // Function to copy the file
    const copyFile = (source, destination) => {
        fs.copyFile(source, destination, (err) => {
            if (err) {
                console.error(`Error copying file: ${err.message}`);
                process.exit(1);
            }
            console.log(`File copied from ${source} to ${destination}`);
        });
    };

    // Start the copy process
    copyFile(source, destination);
});
