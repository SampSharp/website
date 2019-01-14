const path = require("path");
const MiniCssExtractPlugin = require("mini-css-extract-plugin");
const postcssPresetEnv = require("postcss-preset-env");
// We are getting 'process.env.NODE_ENV' from the NPM scripts
// Remember the 'dev' script?
const devMode = process.env.NODE_ENV !== "production";
module.exports = {
  mode: devMode ? "development" : "production",
  entry: ["./Styles/app.scss"],
  output: {
    path: path.resolve(__dirname, "wwwroot"),
    publicPath: "/",
    filename: "js/js.js"
  },
  module: {
    rules: [
      {
        test: /\.(sa|sc)ss$/,
        use: [
          {
            loader: MiniCssExtractPlugin.loader
          },
          {
            loader: "css-loader",
            options: {
              importLoaders: 2
            }
          },
          {
            loader: "postcss-loader",
            options: {
              ident: "postcss",
              plugins: devMode
                ? () => []
                : () => [
                    postcssPresetEnv({
                      // https://github.com/browserslist/browserslist#queries
                      browsers: [">1%"]
                    }),
                    require("cssnano")()
                  ]
            }
          },
          {
            loader: "sass-loader"
          }
        ]
      },
      {
        // Adds support to load images in your CSS rules. It looks for
        // .png, .jpg, .jpeg and .gif
        test: /\.(png|jpe?g|gif)$/,
        use: [
          {
            loader: "file-loader",
            options: {
              name: "[name].[ext]",
              publicPath: "../images",
              emitFile: false
            }
          }
        ]
      },
        {
            // Adds support to load fonts
            test: /\.(otf|eot|svg|ttf|woff|woff2)$/,
            use: [
                {
                    loader: "file-loader",
                    options: {
                        name: "[name].[ext]",
                        publicPath: "../fonts",
                        emitFile: false
                    }
                }
            ]
        }
    ]
  },
  plugins: [
    new MiniCssExtractPlugin({
      filename: devMode ? "css/site.css" : "css/site.min.css"
    })
  ]
};