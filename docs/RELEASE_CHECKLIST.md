# Release checklist

This is a version-controlled project checklist, not a personal scratch note. Update it when the project's release policy changes, and complete it before the first public repository or any binary release.

The repository must remain private until the MOD has been implemented and verified locally in the game.

## Legal and attribution

- [ ] Select and add a source-code license. Public source is not automatically open source.
- [x] Verify the current Lazy Witch's Factory modding and redistribution policy; recheck it before every public release.
- [ ] Confirm that no game artwork, fonts, audio, assemblies, or other copyrighted game assets are included.
- [ ] Confirm the MOD does not unlock unimplemented content or paid/DLC content.
- [ ] Keep the non-affiliation statement in the README.
- [ ] Record licenses and notices for any third-party code added later.

## Repository hygiene

- [ ] Review every tracked file with `git status` and `git diff --cached` before the first commit.
- [ ] Search tracked files for local absolute paths, usernames, email addresses, tokens, and credentials.
- [ ] Confirm `Directory.Build.user.props`, build output, logs, decompiled sources, saves, and generated PNG files are ignored.
- [ ] Confirm generated release archives contain only files intended for end users.

## Release quality

- [ ] Build from a clean checkout using documented steps.
- [ ] Test with the documented game and BepInEx versions.
- [ ] Back up save data before runtime tests that could affect game state.
- [ ] Document installation, removal, configuration, output location, known limitations, and compatibility.
- [ ] Attach checksums to binary releases.

## GitHub configuration

- [ ] Confirm the export feature has been verified locally before changing the GitHub repository to public.
- [ ] Disable unused repository features or configure issue templates as needed.
- [ ] Enable secret scanning and dependency alerts where available.
- [ ] Use tagged releases; do not distribute development builds from arbitrary commits.
