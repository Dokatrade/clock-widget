# Project Instructions

## Before work

Before substantial work, read:

- project-memory/HANDOFF.md
- project-memory/TODO.md
- project-memory/docs/decisions.md
- project-memory/docs/chat-notes.md

If any of these files are missing, create them before continuing unless the task is urgent or explicitly says not to.

## Language

Answer in Russian unless the user explicitly asks for another language.

## Project memory workflow

This project uses file-based memory:

- AGENTS.md = rules for Codex, kept in the project root so Codex auto-discovers it
- project-memory/HANDOFF.md = current project state
- project-memory/TODO.md = current task queue
- project-memory/docs/decisions.md = important decisions
- project-memory/docs/chat-notes.md = compressed notes from chats

Do not store long project history in AGENTS.md. Keep AGENTS.md short and use it as a map.

## Memory file formats

- project-memory/TODO.md uses sections: `Now`, `Next`, `Later`, `Done`.
- project-memory/docs/decisions.md is a dated decision log. Use headings like `## YYYY-MM-DD — Title`, then short decision text and `Причина:`.
- project-memory/docs/chat-notes.md is compressed chat history, not a decision log.

## Work rules

- Do not rewrite unrelated files.
- Preserve existing project style.
- Before changing code, inspect the existing structure.
- Prefer small, reviewable changes.
- Run relevant checks after code changes when possible.
- Do not add new production dependencies without explicit approval.
- Do not delete files unless the user clearly requested it.
- Avoid periodic/background disk writes. The user cares about SSD write load.

## After work

After substantial work:

- Update project-memory/HANDOFF.md if project context changed.
- Update project-memory/TODO.md if task status changed.
- Add important decisions to project-memory/docs/decisions.md.
- Summarize long chat conclusions in project-memory/docs/chat-notes.md.
