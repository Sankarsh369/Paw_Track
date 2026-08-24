/** @type {import('tailwindcss').Config} */
export default {
  content: [
    "./index.html",
    "./src/**/*.{js,ts,jsx,tsx}",
  ],
  theme: {
    extend: {
      colors: {
        chocolate: {
          50: '#FDFBF7',
          100: '#F7F2EA',
          200: '#EADBC8',
          300: '#D4B896',
          400: '#B08968',
          500: '#7F5539',
          600: '#5C3D2E',
          700: '#43281C',
          800: '#2C1810',
          900: '#1C0D07',
        },
        cream: '#FFFDF9',
      },
      fontFamily: {
        sans: ['Plus Jakarta Sans', 'sans-serif'],
        outfit: ['Outfit', 'sans-serif'],
      }
    },
  },
  plugins: [],
}
