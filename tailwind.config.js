/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    "./Views/**/*.{cshtml,html}",
    "./Views/Shared/**/*.{cshtml,html}",
    "./wwwroot/js/**/*.js",
    "./Controllers/**/*.cs" // Include C# files for any dynamic class generation
  ],
  theme: {
    extend: {},
  },
  plugins: [],
}
