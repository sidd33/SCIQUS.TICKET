const fs = require('fs');
const path = require('path');

function walk(dir) {
  let results = [];
  const list = fs.readdirSync(dir);
  list.forEach(function(file) {
    file = path.resolve(dir, file);
    const stat = fs.statSync(file);
    if (stat && stat.isDirectory()) { 
      results = results.concat(walk(file));
    } else { 
      if (file.endsWith('.ts') || file.endsWith('.tsx') || file.endsWith('.d.ts')) {
        results.push(file);
      }
    }
  });
  return results;
}

const screensDir = path.resolve(__dirname, 'node_modules/react-native-screens');
if (!fs.existsSync(screensDir)) {
  console.log('react-native-screens not found');
  process.exit(0);
}

const files = walk(screensDir);
files.forEach(file => {
  let content = fs.readFileSync(file, 'utf8');
  let original = content;
  
  // Replace CT.Type with Type
  content = content.replace(/CT\.([A-Za-z0-9_]+)/g, '$1');
  
  // Replace import type { CodegenTypes as CT ... } with import type { CodegenTypes ... }
  content = content.replace(/CodegenTypes as CT/g, 'CodegenTypes');
  
  if (content !== original) {
    fs.writeFileSync(file, content, 'utf8');
    console.log('Fixed', file);
  }
});
console.log('Done fixing react-native-screens');
