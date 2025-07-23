const fs = require("fs");
const path = require('path');
const { execSync } = require("child_process");

const source = "src/environments/environment.ts";
const target = "src/environments/environment.development.ts";

if (!fs.existsSync(target)) {
  console.log(`Copying ${source} to ${target}...`);
  const targetBasename = path.basename(target);
  execSync(`npx cpy ${source} . --rename=${targetBasename}`);
}
