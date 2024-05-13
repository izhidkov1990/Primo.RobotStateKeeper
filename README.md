Primo.RobotStateKeeper

Описание

Этот проект включает в себя реализацию чтения состояния робота из JSON-файла. Он содержит класс StateReader, который считывает состояние робота и обновляет его в модели RobotStateModel.

Свойства

•	JsonFilePath: Путь к JSON-файлу, который содержит состояние робота (например, c:\folder\state.json)

•	RobotState: Самое последнее состояние робота, считанное из JSON-файла. Это свойство обновляется каждый раз, когда компонент считывает состояние из файла.

