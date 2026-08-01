const fs = require('fs');
const path = require('path');

const pkgPath = path.resolve(__dirname, 'node_modules/react-native-screens/package.json');
if (fs.existsSync(pkgPath)) {
  const pkg = JSON.parse(fs.readFileSync(pkgPath, 'utf8'));
  if (pkg.codegenConfig) {
    delete pkg.codegenConfig;
    fs.writeFileSync(pkgPath, JSON.stringify(pkg, null, 2), 'utf8');
    console.log('Removed codegenConfig from react-native-screens/package.json');
  } else {
    console.log('No codegenConfig found');
  }
} else {
  console.log('react-native-screens/package.json not found');
}
