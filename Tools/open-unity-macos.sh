#!/bin/bash
# Launch this project with native Git when Unity Package Manager runs under Rosetta.
set -euo pipefail

project_path="$(cd "$(dirname "$0")/.." && pwd)"
editor_version="$(sed -n 's/^m_EditorVersion: //p' "$project_path/ProjectSettings/ProjectVersion.txt")"
unity_binary="/Applications/Unity/Hub/Editor/$editor_version/Unity.app/Contents/MacOS/Unity"
if [[ ! -x "$unity_binary" ]]; then
    echo "Unity Editor not found: $unity_binary" >&2
    exit 1
fi
if [[ -e "$project_path/Temp/UnityLockfile" ]]; then
    echo "Close this project's Unity Editor before using this launcher." >&2
    exit 1
fi

if [[ "$(/usr/bin/uname -m)" == "arm64" ]]; then
    git_tools="$(/usr/bin/mktemp -d "${TMPDIR:-/tmp}/myframework-unity-git.XXXXXX")"
    trap '/bin/rm -rf "$git_tools"' EXIT
    cat > "$git_tools/git" <<'EOF'
#!/bin/sh
exec /usr/bin/arch -arm64 /usr/bin/git "$@"
EOF
    /bin/chmod +x "$git_tools/git"
    export PATH="$git_tools:$PATH"
fi

"$unity_binary" -projectPath "$project_path" "$@"
