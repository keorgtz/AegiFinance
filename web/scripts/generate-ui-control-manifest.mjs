import { readdir, readFile, writeFile, mkdir } from "node:fs/promises";
import path from "node:path";

const root = process.cwd();
const writeMode = process.argv.includes("--write");
const sourceRoots = ["app", "components"];
const tags = new Set(["Button", "Input", "Select", "Textarea", "Tab", "Link", "button", "input", "select", "textarea"]);

async function filesIn(directory) {
  const entries = await readdir(directory, { withFileTypes: true });
  const nested = await Promise.all(entries.map(async (entry) => {
    const full = path.join(directory, entry.name);
    if (entry.isDirectory()) return filesIn(full);
    return entry.isFile() && full.endsWith(".tsx") ? [full] : [];
  }));
  return nested.flat();
}

function slug(value) {
  return value.replace(/\\/g, "/").replace(/\.tsx$/, "").replace(/[^a-zA-Z0-9]+/g, ".").replace(/^\.|\.$/g, "").toLowerCase();
}

function openingTags(source) {
  const matches = [];
  const regex = /<([A-Za-z][A-Za-z0-9]*)\b/g;
  let match;
  while ((match = regex.exec(source))) {
    if (!tags.has(match[1])) continue;
    let quote = null;
    let braces = 0;
    let end = regex.lastIndex;
    for (; end < source.length; end += 1) {
      const char = source[end];
      if (quote) {
        if (char === quote && source[end - 1] !== "\\") quote = null;
      } else if (char === '"' || char === "'") quote = char;
      else if (char === "{") braces += 1;
      else if (char === "}") braces = Math.max(0, braces - 1);
      else if (char === ">" && braces === 0) break;
    }
    matches.push({ tag: match[1], start: match.index, nameEnd: match.index + match[0].length, end });
  }
  return matches;
}

function attribute(text, name) {
  return text.match(new RegExp(`${name}=["']([^"']+)["']`))?.[1];
}

const files = (await Promise.all(sourceRoots.map((dir) => filesIn(path.join(root, dir))))).flat()
  .filter((file) => !file.includes(`${path.sep}components${path.sep}ui${path.sep}`));
const manifest = [];
const missing = [];

for (const file of files) {
  let source = await readFile(file, "utf8");
  source = source
    .replace(/<Butto (controlKey|data-ui-control)="([^"]+)"n\b/g, '<Button $1="$2"')
    .replace(/<Inpu (controlKey|data-ui-control)="([^"]+)"t\b/g, '<Input $1="$2"')
    .replace(/<Selec (controlKey|data-ui-control)="([^"]+)"t\b/g, '<Select $1="$2"')
    .replace(/<Textare (controlKey|data-ui-control)="([^"]+)"a\b/g, '<Textarea $1="$2"')
    .replace(/<Ta (controlKey|data-ui-control)="([^"]+)"b\b/g, '<Tab $1="$2"')
    .replace(/<Lin (controlKey|data-ui-control)="([^"]+)"k\b/g, '<Link $1="$2"')
    .replace(/<butto (controlKey|data-ui-control)="([^"]+)"n\b/g, '<button $1="$2"')
    .replace(/<inpu (controlKey|data-ui-control)="([^"]+)"t\b/g, '<input $1="$2"')
    .replace(/<selec (controlKey|data-ui-control)="([^"]+)"t\b/g, '<select $1="$2"')
    .replace(/<textare (controlKey|data-ui-control)="([^"]+)"a\b/g, '<textarea $1="$2"');
  const relative = path.relative(root, file);
  const matches = openingTags(source);
  let offset = 0;
  const counters = new Map();
  for (const original of matches) {
    const counterKey = original.tag.toLowerCase();
    const count = (counters.get(counterKey) ?? 0) + 1;
    counters.set(counterKey, count);
    const start = original.start + offset;
    const end = original.end + offset;
    const opening = source.slice(start, end + 1);
    const keyAttribute = original.tag[0] === original.tag[0].toUpperCase() && !["Link"].includes(original.tag)
      ? "controlKey" : "data-ui-control";
    const desiredKey = `ui.${slug(relative)}.${original.tag.toLowerCase()}.${count}`;
    let key = attribute(opening, keyAttribute) ?? attribute(opening, "data-ui-control");
    const hasDynamicKey = opening.includes(`${keyAttribute}={`) || opening.includes("data-ui-control={`");
    if (!key && hasDynamicKey) continue;
    if (writeMode && key?.startsWith("ui.") && key !== desiredKey) {
      const updatedOpening = opening.replace(`${keyAttribute}="${key}"`, `${keyAttribute}="${desiredKey}"`);
      source = source.slice(0, start) + updatedOpening + source.slice(end + 1);
      offset += updatedOpening.length - opening.length;
      key = desiredKey;
    }
    if (!key) {
      key = desiredKey;
      if (writeMode) {
        const insertion = ` ${keyAttribute}="${key}"`;
        const position = original.nameEnd + offset;
        source = source.slice(0, position) + insertion + source.slice(position);
        offset += insertion.length;
      } else {
        missing.push(`${relative}: ${original.tag} #${count}`);
      }
    }
    const label = attribute(opening, "label") ?? attribute(opening, "aria-label") ?? attribute(opening, "name") ?? `${original.tag} ${count}`;
    const permission = attribute(opening, "permission") ?? attribute(opening, "data-ui-permission");
    const systemRequired = /logout|cerrar sesi[oó]n|close dialog|cerrar di[aá]logo/i.test(`${key} ${label}`);
    manifest.push({
      controlKey: key,
      label,
      module: slug(relative).split(".").find((part) => !["app", "components", "modules"].includes(part)) ?? "system",
      controlType: original.tag.toLowerCase(),
      requiredPermissionCode: permission ?? null,
      isSystemRequired: systemRequired,
    });
  }
  if (writeMode) await writeFile(file, source, "utf8");
}

const navigationSource = await readFile(path.join(root, "lib", "navigation.ts"), "utf8");
for (const match of navigationSource.matchAll(/href:\s*"([^"]+)"[\s\S]*?label:\s*"([^"]+)"[\s\S]*?permission:\s*"([^"]+)"/g)) {
  const [, href, label, permission] = match;
  manifest.push({
    controlKey: `navigation.${href.slice(1).replaceAll("/", ".")}.open`,
    label,
    module: "navigation",
    controlType: "link",
    requiredPermissionCode: permission,
    isSystemRequired: false,
  });
}

if (missing.length) {
  console.error(`Interactive controls without a stable key:\n${missing.join("\n")}`);
  process.exit(1);
}

const keys = new Set();
for (const item of manifest) {
  if (keys.has(item.controlKey)) throw new Error(`Duplicate UI control key: ${item.controlKey}`);
  keys.add(item.controlKey);
}

await mkdir(path.join(root, "generated"), { recursive: true });
await writeFile(path.join(root, "generated", "ui-control-manifest.json"), `${JSON.stringify(manifest, null, 2)}\n`, "utf8");
console.log(`Verified ${manifest.length} permission-aware UI controls.`);
