/// <reference types="vitest" />
import { defineConfig } from "vitest/config";

export default defineConfig({
    test: {
        environment: "node",

       
        pool: "threads",
        fileParallelism: true,

       
        setupFiles: ["tests/setupXrmMock.ts"],

        // optional but recommended
        clearMocks: true,
        restoreMocks: true,
        mockReset: true,

        // avoid accidental long hangs
        testTimeout: 10000,
        hookTimeout: 10000,

        fakeTimers: {
            toFake: ["setTimeout", "clearTimeout"]
        }
    }
});
