# Release checklist

This is a version-controlled project checklist, not a personal scratch note. Update it when the project's release policy changes, and complete it before any binary release.

The GitHub repository is intended to be public and to hold source, development documentation, issues, changelog history, and Thunderstore page assets. End-user releases are distributed through Thunderstore only after the MOD has been implemented and verified locally in the game.

## Legal and attribution

- [ ] Select and add a source-code license. Public source is not automatically open source.
- [x] Verify the current Lazy Witch's Factory modding and redistribution policy; recheck it before every public release.
- [ ] Confirm that no extracted game artwork, fonts, audio, assemblies, or other copyrighted game assets are included. MOD demonstration screenshots are reviewed separately under the official policy.
- [ ] Confirm the MOD does not unlock unimplemented content or paid/DLC content.
- [ ] Keep the non-affiliation statement in the README.
- [ ] Record licenses and notices for any third-party code added later.

## Repository hygiene

- [ ] Review every tracked file with `git status` and `git diff --cached` before the first commit.
- [ ] Search tracked files for local absolute paths, usernames, email addresses, tokens, and credentials.
- [ ] Confirm `Directory.Build.user.props`, build output, logs, decompiled sources, saves, and generated PNG files are ignored.
- [ ] Confirm generated release archives contain only files intended for end users.
- [ ] Review the public GitHub repository as an unauthenticated visitor, including image assets and linked documents.

## Release quality

- [ ] Build from a clean checkout using documented steps.
- [ ] Test with the documented game and BepInEx versions.
- [ ] Back up save data before runtime tests that could affect game state.
- [ ] Document installation, removal, configuration, output location, known limitations, and compatibility.
- [ ] Attach checksums to binary releases.

## Thunderstore package

- [ ] Confirm the export feature has been verified locally before uploading a public package.
- [ ] Include `manifest.json`, `README.md`, and a 256×256 `icon.png` at the root of the package ZIP.
- [ ] Validate the package manifest and preview the README in Thunderstore.
- [ ] Set `website_url` to the public GitHub repository after its URL is known.
- [ ] Replace relative image paths in `thunderstore/README.md` with public GitHub raw-image URLs and verify them while signed out.
- [ ] Use tagged releases; do not distribute development builds from arbitrary commits.
