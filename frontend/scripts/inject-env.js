const fs = require ("fs");
const path = require ("path");

try
{
  require("dotenv").config();
}
catch (err) {}

const files = [
  path.join (__dirname, "../src/environments/environment.ts"),
  path.join (__dirname, "../src/environments/environment.prod.ts")
];

const authUrl = process.env.AUTH_URL;
const authKey = process.env.AUTH_KEY;

if (!authUrl || !authKey)
{
  console.error ("Missing env variables: AUTH_URL and/or AUTH_KEY");
  process.exit (1);
}

let success = false;
files.forEach (
  path =>
  {
    const examplePath = path.replace (/\.ts/g, ".example.ts");
    if (!fs.existsSync (examplePath))
    {
      console.warn (`${examplePath} does not exist, skipping...`);
      return;
    }
    
    let content = fs.readFileSync (examplePath, "utf8");
    const original = content;
    
    content = content.replace (/__AUTH_URL__/g, authUrl);
    content = content.replace (/__AUTH_KEY__/g, authKey);
    
    if (content === original)
      console.log (`No placeholders found in ${path}, no changes made`);
    else
    {
      fs.writeFileSync (path, content, "utf8");
      console.log (`Injected auth credentials into ${path}`);
      success = true;
    }
  }
);

if (!success)
  console.log ("No env files were updated. Ensure they contain __AUTH_URL__ and __AUTH_KEY__ placeholders.");