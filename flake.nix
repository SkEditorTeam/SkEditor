{
  inputs = {
    nixpkgs.url = "github:NixOS/nixpkgs/nixos-unstable";
    flake-parts.url = "github:hercules-ci/flake-parts";
    flake-compat.url = "https://flakehub.com/f/edolstra/flake-compat/1.tar.gz";
  };

  outputs =
    inputs@{ nixpkgs, flake-parts, ... }:
    flake-parts.lib.mkFlake { inherit inputs; } {
      systems = nixpkgs.lib.systems.flakeExposed;
      perSystem =
        {
          pkgs,
          ...
        }:
        {
          formatter = pkgs.nixfmt-rfc-style;
          devShells.default = pkgs.mkShell {
            buildInputs = [
              pkgs.dotnet-sdk_8
            ];
          };
        };
    };
}
