#!/bin/sh
# Compiles the project's C# against the Unity 6 reference assemblies, so
# errors show up here instead of only in the editor console.
#
#   sh check_scripts.sh          both passes, errors only
#   sh check_scripts.sh -v       include warnings
#
# Pass 1 builds the runtime scripts the way the player build does, with no
# UnityEditor reference, so editor-only API used by mistake is caught.
# Pass 2 builds the Editor folder on top of that.

UE="C:/Program Files/Unity/Hub/Editor/6000.4.6f1/Editor/Data"
OUT="${TMP:-/tmp}/skydash-build"
mkdir -p "$OUT"

VERBOSE=0
[ "$1" = "-v" ] && VERBOSE=1

common_flags() {
  echo '-target:library'
  echo '-nologo'
  echo '-langversion:9.0'
  echo '-nostdlib+'
  echo '-define:UNITY_2023_1_OR_NEWER;UNITY_6000_0_OR_NEWER;UNITY_ANDROID;UNITY_EDITOR'
  echo '-nowarn:0649,0414'
  for f in "$UE/NetStandard/ref/2.1.0"/*.dll; do echo "-r:\"$f\""; done
  for f in Library/ScriptAssemblies/UnityEngine.UI.dll \
           Library/ScriptAssemblies/Unity.TextMeshPro.dll; do
    [ -f "$f" ] && echo "-r:\"$f\""
  done
}

# --- pass 1: runtime scripts -------------------------------------------------
RSP="$OUT/runtime.rsp"
{
  common_flags
  echo "-out:\"$OUT/Assembly-CSharp.dll\""
  for f in "$UE/Managed/UnityEngine"/UnityEngine*.dll; do
    case "$f" in *UnityEditor*) continue ;; esac
    echo "-r:\"$f\""
  done
  find Assets/Scripts -name '*.cs' -not -path '*/Editor/*' | sed 's/.*/"&"/'
} > "$RSP"
RUNTIME_LOG="$OUT/runtime.log"
"$UE/NetCoreRuntime/dotnet.exe" "$UE/DotNetSdkRoslyn/csc.dll" "@$RSP" > "$RUNTIME_LOG" 2>&1
RUNTIME_RC=$?

# --- pass 2: editor scripts --------------------------------------------------
EDITOR_LOG="$OUT/editor.log"
EDITOR_RC=0
if find Assets/Scripts -path '*/Editor/*' -name '*.cs' | grep -q .; then
  RSP2="$OUT/editor.rsp"
  {
    common_flags
    echo "-out:\"$OUT/Assembly-CSharp-Editor.dll\""
    for f in "$UE/Managed/UnityEngine"/*.dll; do echo "-r:\"$f\""; done
    echo "-r:\"$OUT/Assembly-CSharp.dll\""
    find Assets/Scripts -path '*/Editor/*' -name '*.cs' | sed 's/.*/"&"/'
  } > "$RSP2"
  "$UE/NetCoreRuntime/dotnet.exe" "$UE/DotNetSdkRoslyn/csc.dll" "@$RSP2" > "$EDITOR_LOG" 2>&1
  EDITOR_RC=$?
fi

# --- report ------------------------------------------------------------------
if [ "$VERBOSE" -eq 1 ]; then
  grep -hE 'error CS|warning CS' "$RUNTIME_LOG" "$EDITOR_LOG" 2>/dev/null | grep -v 'CS2023'
else
  grep -hE 'error CS' "$RUNTIME_LOG" "$EDITOR_LOG" 2>/dev/null
fi

ERRORS=$(grep -hE 'error CS' "$RUNTIME_LOG" "$EDITOR_LOG" 2>/dev/null | wc -l | tr -d ' ')
if [ "${ERRORS:-0}" -eq 0 ] && [ "$RUNTIME_RC" -eq 0 ] && [ "$EDITOR_RC" -eq 0 ]; then
  echo "compile OK: runtime + editor scripts"
  exit 0
fi

echo "compile FAILED: $ERRORS error(s)"
exit 1
