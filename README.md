# GitHub Copilot CLI for Beginners — Course Summary

> A complete guide to GitHub Copilot CLI, from installation through advanced workflows combining agents, skills, and MCP servers.

---

## Chapter 00: Quick Start

**Goal:** Get GitHub Copilot CLI installed, authenticated, and verified in ~10 minutes.

### Prerequisites
- A GitHub account with an active Copilot subscription (Individual, Business, Enterprise, or GitHub Education/free for students and teachers).
- Basic terminal familiarity (`cd`, `ls`).

### Installation Options
- **GitHub Codespaces** (zero setup): Fork the repo, open a Codespace, and you're ready.
- **Local — npm:** `npm install -g @github/copilot`
- **Local — Homebrew (macOS/Linux):** `brew install copilot-cli`
- **Local — WinGet (Windows):** `winget install GitHub.Copilot`
- **Local — Install script:** `curl -fsSL https://gh.io/copilot-install | bash`

### Authentication
```bash
copilot
> /login
```
A one-time device authorization flow links your GitHub account. The session persists until the token expires.

### Verification
```bash
> Say hello and tell me what you can help with
> /exit
```
Test the included sample Python book app: `cd samples/book-app-project && python book_app.py list`

### Key Takeaways
- Codespaces is the fastest path to getting started.
- Authentication is a one-time step.
- The sample `book-app-project` is used throughout the entire course.

---

## Chapter 01: First Steps

**Goal:** Experience Copilot CLI's core value and master the three interaction modes.

### The Three Modes

| Mode | Flag/Command | Best For |
|------|-------------|----------|
| **Interactive** | `copilot` | Exploration, multi-turn conversations |
| **Plan** | `/plan` or `Shift+Tab` | Complex tasks — review before execution |
| **Programmatic** | `copilot -p "<prompt>"` | Automation, scripts, one-shot answers |

**Start with Interactive mode** — context builds naturally and follow-ups are easy.

### Essential Slash Commands

| Command | Purpose |
|---------|---------|
| `/ask` | Quick off-the-record question (doesn't affect session history) |
| `/clear` | Wipe conversation and start fresh |
| `/plan` | Map out an implementation step-by-step |
| `/research` | Deep research from GitHub and web sources |
| `/model` | View or switch AI model |
| `/help` | List all available commands |
| `/exit` | End the session |

### Three Demo Highlights
1. **Code Review in Seconds**: `> Review @samples/book-app-project/book_app.py for code quality issues`
2. **Explain Confusing Code**: `> Explain what @samples/book-app-project/books.py does in simple terms`
3. **Generate Working Code**: Describe a function in plain English and get working code instantly.

### Remote Sessions
```bash
copilot --remote  # Monitor and steer a CLI session from mobile or browser
```

### Key Takeaways
- Interactive mode is conversational and context-aware.
- Plan mode maps the route before driving.
- Programmatic mode (`-p`) is for scripts and one-off answers.
- `Shift+Tab` cycles between Interactive → Plan → Autopilot.

---

## Chapter 02: Context and Conversations

**Goal:** Use the `@` syntax to give Copilot CLI deep codebase understanding, and manage sessions across multiple days.

### The @ Syntax

| Pattern | Example |
|---------|---------|
| `@file.py` | `Review @books.py` |
| `@folder/` | `Review @samples/book-app-project/` |
| `@file1.py @file2.py` | `Compare @book_app.py @books.py` |
| `@image.png` | Paste or reference screenshots for UI analysis |

Cross-file analysis reveals bugs and patterns that single-file review misses entirely.

### Session Management

```bash
# Start a named session
copilot --name book-app-review

# Continue the most recent session
copilot --continue

# Resume a specific session
copilot --resume=book-app-review

# Rename the current session
> /rename book-app-review

# Delete sessions
> /session delete
```

### Context Window Commands

| Command | Purpose |
|---------|---------|
| `/context` | Show token usage |
| `/compact` | Summarize history to free up tokens |
| `/clear` | Abandon session, start fresh |
| `/new` | Save session to history, start fresh |
| `/rewind` | Roll back to any earlier point |

### Best Practices for Large Codebases
- Be specific (`@books.py`) rather than broad (`@project/`) when possible.
- Use `/new` or `/clear` when switching topics.
- Split work into one session per feature or topic.

### Key Takeaways
- `@` syntax is the foundation of context-aware assistance.
- Sessions auto-save; use `--continue` or `--resume` to pick up where you left off.
- Context windows have limits — manage them proactively.

---

## Chapter 03: Development Workflows

**Goal:** Use Copilot CLI inside the five everyday development workflows.

### Workflow Overview

| Workflow | Best Prompt Pattern |
|----------|-------------------|
| **Code Review** | `@file.py Review for [specific concern]` |
| **Refactoring** | `@file.py Refactor X to use Y pattern` |
| **Debugging** | `@file.py Users report [symptom]. Debug why.` |
| **Test Generation** | `@file.py Generate pytest tests including edge cases` |
| **Git Integration** | `copilot -p "Generate commit message for: $(git diff --staged)"` |

### Highlights

**Code Review** — Use `/review` to invoke the built-in code-review agent on staged/unstaged changes. Use `@folder/` with a severity template for project-wide checklists.

**Refactoring** — Generate tests *first*, then refactor safely:
```bash
> @books.py Before refactoring, generate tests for current behavior
> Now refactor BookCollection to use a context manager for file operations
```

**Debugging** — Describe the symptom, not a vague request:
```
> @books_buggy.py Users report searching for "The Hobbit" returns no results. Debug why.
```

**Test Generation** — A single prompt can produce 15+ tests including edge cases, special characters, and data persistence scenarios.

**Git Integration**:
```bash
copilot -p "Generate a conventional commit message for: $(git diff --staged)"
copilot -p "Generate a PR description from: $(git log main..HEAD --oneline)"
> /pr [view|create|fix|auto]   # Work with PRs in interactive mode
> /diff                         # Review all session changes before committing
> /delegate Add validation      # Hand off a task to the cloud agent
```

### Complete Bug-Fix Workflow Summary

| Step | Tool |
|------|------|
| Understand bug | `> [describe symptom] @file.py Analyze the likely cause` |
| Fix | `> Show me the function and fix the issue` |
| Test | `> Generate tests for [specific scenarios]` |
| Stage | `git add .` |
| Commit message | `copilot -p "Generate commit message for: $(git diff --staged)"` |

### Key Takeaways
- Be specific in prompts — describe symptoms, not vague requests.
- Generate tests before refactoring.
- `/review` is optimized for high signal-to-noise code review output.
- Git integration can automate commit messages and PR descriptions.

---

## Chapter 04: Agents and Custom Instructions

**Goal:** Use specialized AI agents for domain-specific tasks and encode team standards in configuration files.

### Built-in Agents

| Agent | How to Invoke | Purpose |
|-------|--------------|---------|
| **Plan** | `/plan` or `Shift+Tab` | Step-by-step implementation planning |
| **Code-review** | `/review` | Focused review of staged/unstaged changes |
| **Init** | `/init` | Generate project config files |
| **Explore** | *Automatic* | Internal codebase analysis |
| **Task** | *Automatic* | Run tests, builds, lints |

### Custom Agents

Agent files are Markdown files with a `.agent.md` extension.

**Minimal agent:**
```markdown
---
name: my-reviewer
description: Code reviewer focused on bugs and security
---

You are a code reviewer focused on finding bugs and security issues.
```

**File locations:**
- `.github/agents/` — project-specific, version-controlled, shared with team
- `~/.copilot/agents/` — personal, available in all projects

**Using agents:**
```bash
copilot
> /agent              # Pick from list interactively

copilot --agent python-reviewer   # Launch directly with an agent
```

### Project Configuration Files

| File | Scope | Notes |
|------|-------|-------|
| `AGENTS.md` | Project root | Cross-platform standard; works with Copilot and other AI tools |
| `.github/copilot-instructions.md` | Project | GitHub Copilot specific |
| `.github/instructions/*.instructions.md` | Project | Granular, topic-specific |

Use `/init` to let Copilot scan your project and generate config files automatically.

Add `--no-custom-instructions` to skip project config in a session.

### Key Takeaways
- Agents are specialists — the same prompt gets dramatically better output from a targeted agent.
- Built-in agents (`/plan`, `/review`) cover most daily use.
- Custom agents live in `.agent.md` files and require a `description` field.
- Instruction files apply automatically to every session without needing to invoke an agent.

---

## Chapter 05: Skills System

**Goal:** Create task-specific instructions that Copilot automatically applies when your prompts match them.

### How Skills Work

Skills are folders containing a `SKILL.md` file. Copilot reads your prompt and **automatically loads matching skills** — no slash command required. You can also invoke them directly:

```bash
> /security-audit Check the API endpoints
> /code-checklist Review books.py
```

Or combine multiple skills in one message:
```bash
> Check @book_app.py with /code-checklist and also run /generate-tests for it
```

### Agents vs Skills vs MCP

| Tool | Activation | Best For |
|------|-----------|---------|
| **Agents** | Explicit (`/agent`) | Broad domain expertise across many tasks |
| **Skills** | Automatic (prompt-matched) | Specific, repeatable tasks with detailed steps |
| **MCP** | Automatic (configured servers) | Live data from external APIs |

### Skill File Structure

```
.github/skills/
└── security-audit/
    └── SKILL.md        # Required; must be named exactly SKILL.md
```

**SKILL.md format:**
```markdown
---
name: security-audit
description: Security review checking OWASP Top 10 vulnerabilities, SQL injection, XSS, auth issues
---

# Security Audit

Check for:
- SQL injection
- Hardcoded credentials
- Missing input validation
...
```

**The `description` field is critical** — it's how Copilot decides whether to load your skill. Include keywords that match how you naturally ask questions.

### Skill Locations

| Path | Scope |
|------|-------|
| `.github/skills/` | Project/team (version controlled) |
| `~/.copilot/skills/` | Personal (all projects) |

### Managing Skills

```bash
> /skills list          # Show all installed skills
> /skills info <name>   # Details about a skill
> /skills reload        # Pick up edits without restarting
```

### Installing Community Skills

```bash
gh skill install github/awesome-copilot               # Browse and select
gh skill install github/awesome-copilot code-checklist # Install specific skill
```

### Key Takeaways
- Skills auto-trigger from prompt matching — just ask naturally.
- The `description` field drives automatic discovery; make it keyword-rich.
- Use `/skills reload` after editing a `SKILL.md` to pick up changes.
- The file must be named exactly `SKILL.md`.

---

## Chapter 06: MCP Servers

**Goal:** Connect Copilot to external services so it can access live data from GitHub, filesystems, and documentation.

### What MCP Does

Without MCP, Copilot can only see files you explicitly share with `@`. With MCP, it can proactively explore your project, check your GitHub repo, and look up documentation automatically.

### Built-in: GitHub MCP

No setup required. The GitHub MCP server is included by default.

```bash
copilot
> List the recent commits in this repository  # MCP in action
> /mcp show                                   # Check configured servers
```

### Configuring Additional Servers

Servers are configured in `~/.copilot/mcp-config.json` (user-level) or `.mcp.json` (project-level).

**Discover servers interactively:**
```bash
> /mcp search   # Opens a guided picker — no JSON editing required
```

**Filesystem server** (lets Copilot browse project files):
```json
{
  "mcpServers": {
    "filesystem": {
      "type": "local",
      "command": "npx",
      "args": ["-y", "@modelcontextprotocol/server-filesystem", "."],
      "tools": ["*"]
    }
  }
}
```

**Context7 server** (up-to-date library documentation):
```json
{
  "mcpServers": {
    "context7": {
      "type": "local",
      "command": "npx",
      "args": ["-y", "@upstash/context7-mcp"],
      "tools": ["*"]
    }
  }
}
```

Both Context7 and the Microsoft Learn MCP server require no API key or account.

### Managing Servers

```bash
> /mcp show                    # Status of all servers
> /mcp enable <server-name>    # Enable a server
> /mcp disable <server-name>   # Disable a server
copilot mcp list               # From terminal (no session needed)
```

### Multi-Server Workflow Example

In a single session, you can combine:
- **Filesystem MCP** to read project files and check type hints
- **GitHub MCP** to review commit history and open issues
- **Context7 MCP** to fetch current documentation

This creates a complete health dashboard or issue-to-PR workflow without leaving the terminal.

### Key Takeaways
- GitHub MCP is built-in — no configuration needed.
- Filesystem and Context7 are the most commonly added servers.
- Use `/mcp search` for guided, no-JSON-required server setup.
- Multi-server workflows aggregate data from multiple sources in a single session.

---

## Chapter 07: Putting It All Together

**Goal:** Combine agents, skills, and MCP into unified workflows that take a feature from idea to merged PR in one session.

### The Integration Pattern

```
Gather Context (MCP) → Analyze & Plan (Agents) → Execute (Skills) → Complete (MCP)
```

### Idea to Merged PR in One Session

```bash
copilot
> Describe the feature needed

> /agent               # Switch to python-reviewer
> Design the method    # Expert code design

> /agent               # Switch to pytest-helper
> Design test cases    # Expert test design

> Implement the feature
> Generate comprehensive tests
> /review              # Review changes
> /pr auto             # Create pull request
```

### Automation: Pre-Commit Hook

Set up a git hook to automatically run security reviews before every commit:

```bash
cat > .git/hooks/pre-commit << 'EOF'
#!/bin/bash
STAGED=$(git diff --cached --name-only --diff-filter=ACM | grep -E '\.py$')
if [ -n "$STAGED" ]; then
  for file in $STAGED; do
    REVIEW=$(timeout 60 copilot --allow-all -p "Quick security review of @$file - critical issues only")
    if echo "$REVIEW" | grep -qi "CRITICAL"; then
      echo "Critical issues found in $file:"
      echo "$REVIEW"
      exit 1
    fi
  done
fi
EOF
chmod +x .git/hooks/pre-commit
```

### Best Practices

1. **Context first**: Gather MCP data before asking for analysis.
2. **Agents analyze; skills execute**: Use the right tool for the job.
3. **One session per feature**: Use `/rename` to label sessions and `/exit` to close them cleanly.
4. **Encode workflows in the repo**: Use `.github/agents/`, `.github/skills/`, and `.github/copilot-instructions.md` so the whole team benefits automatically.

### Key Takeaways
- You don't need agents, skills, or MCP to be productive — the core workflow from Chapters 00–03 is sufficient.
- Agents, skills, and MCP multiply effectiveness when combined.
- Automate with hooks and CI/CD to catch issues before they reach production.
- Sharing agents and skills in the repo distributes workflows to the whole team for free.

---

## Course Summary Table

| Chapter | Core Skill | Key Command(s) |
|---------|-----------|---------------|
| 00 Quick Start | Install & authenticate | `copilot`, `/login` |
| 01 First Steps | Three interaction modes | `copilot -p`, `/plan`, `/exit` |
| 02 Context & Conversations | @ file references, sessions | `@file.py`, `--continue`, `--resume` |
| 03 Development Workflows | Review, refactor, debug, test, git | `/review`, `/diff`, `/delegate` |
| 04 Agents | Specialized AI personas | `/agent`, `--agent`, `.agent.md` |
| 05 Skills | Auto-triggered task instructions | `/skills list`, `SKILL.md` |
| 06 MCP Servers | External service connections | `/mcp show`, `mcp-config.json` |
| 07 Putting It Together | End-to-end production workflows | `/pr`, hooks, combined agents+skills+MCP |

### Additional Resources
- [GitHub Copilot CLI Documentation](https://docs.github.com/copilot/concepts/agents/about-copilot-cli)
- [CLI Command Reference](https://docs.github.com/en/copilot/reference/cli-command-reference)
- [Community Skills & Agents](https://github.com/github/awesome-copilot)
- [MCP Server Registry](https://github.com/modelcontextprotocol/servers)
