import { defineConfig } from "vite";

export default defineConfig({
    build: {
        lib: {
            entry: "src/index.ts",
            name: "DPE",
            formats: ["iife"],
            fileName: () => "dpe.policyClient.js"
        },
        outDir: "dist",
        emptyOutDir: true
    }
});
