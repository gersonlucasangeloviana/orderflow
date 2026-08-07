#!/usr/bin/env sh
set -eu
command -v plantuml >/dev/null || { echo 'Instale PlantUML para gerar SVGs.' >&2; exit 1; }
plantuml -tsvg docs/uml/*.puml
